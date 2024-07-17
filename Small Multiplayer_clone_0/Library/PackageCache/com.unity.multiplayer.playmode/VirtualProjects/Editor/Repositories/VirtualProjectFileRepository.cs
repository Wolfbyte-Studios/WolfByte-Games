using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.Multiplayer.Playmode.Common.Editor;

namespace Unity.Multiplayer.Playmode.VirtualProjects.Editor
{
    static class VirtualProjectFileRepository
    {
        internal static readonly string[] PathsRequiredForProject = {
            "Assets",
            "ProjectSettings",
            "Packages",
            "Temp",
        };

        static readonly IReadOnlyCollection<string> PathsRequiringSymlink = new[]
        {
            "Assets",
            "ProjectSettings",
        };

        public static bool CreateProject(FileSystemDelegates fileSystemDelegates, VirtualProjectIdentifier identifier,
            out string cloneProjectPath, out CreateAPIErrorInfo errorState)
        {
            errorState = default;
            cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);

            // This is to verify if the directory on Windows respects the supported path length
            if (!fileSystemDelegates.IsPathValidFunc(cloneProjectPath))
            {
                errorState = new CreateAPIErrorInfo { Error = CreateAPIError.ProjectUnableToBeCreated };
                return false;
            }

            fileSystemDelegates.CreateDirectoryFunc(cloneProjectPath);

            var mainProjectDirectory = fileSystemDelegates.GetParentPathFunc(Paths.GetCurrentProjectDataPath());
            var cloneProjectPathCopyForLambda = cloneProjectPath;
            foreach (var path in PathsRequiringSymlink)
            {
                var source = Path.Combine(mainProjectDirectory, path);
                var destination = Path.Combine(cloneProjectPathCopyForLambda, path);
                if (!fileSystemDelegates.SymlinkFileFunc(source, destination, out errorState))
                {
                    return false;
                }
            }

            // Normally Unity creates this automatically, but there was a short period of time when
            // that was not the case. Once we depend on >=2023.3.0b8 we'll be able to remove this.
            fileSystemDelegates.CreateDirectoryFunc(Path.Combine(cloneProjectPath, "Temp"));

            fileSystemDelegates.CreateDirectoryFunc(Path.Combine(cloneProjectPath, "Packages"));

            // Copy packages-lock.json as-is. Even if it contained relative paths, those will get
            // updated automatically once the new manifest is loaded.
            fileSystemDelegates.CopyFileFunc(
                Path.Combine(mainProjectDirectory, "Packages", "packages-lock.json"),
                Path.Combine(cloneProjectPath, "Packages", "packages-lock.json"));

            return true;
        }

        public static void DeleteProject(FileSystemDelegates fileSystemDelegates, VirtualProjectIdentifier identifier)
        {
            var cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            fileSystemDelegates.DeleteDirectoryFunc(cloneProjectPath);
        }

        public static bool HasProject(FileSystemDelegates fileSystemDelegates, VirtualProjectIdentifier identifier, out GetAPIErrorInfo errorState)
        {
            foreach (var projectDirectoryName in fileSystemDelegates.GetDirectoryNamesFunc(Paths.CurrentProjectVirtualProjectsFolder))
            {
                var hasParse = VirtualProjectIdentifier.TryParse(projectDirectoryName, out var identifierFromDirectory);
                var isSpecifiedDirectory = hasParse && identifierFromDirectory == identifier;
                if (isSpecifiedDirectory)
                {
                    if (!HasRequiredDirectoriesForClone(fileSystemDelegates, identifier, out var missingDirectories))
                    {
                        errorState = new GetAPIErrorInfo
                        {
                            Error = GetAPIError.MissingRequiredDirectories,
                            Directories = missingDirectories,
                        };
                        return false;
                    }

                    errorState = default;
                    return true;
                }
            }

            errorState = new GetAPIErrorInfo { Error = GetAPIError.ProjectNotFound };
            return false;
        }

        public static VirtualProjectIdentifier[] GetProjects(FileSystemDelegates fileSystemDelegates)
        {
            var results = new List<VirtualProjectIdentifier>();
            var paths = fileSystemDelegates.GetDirectoryNamesFunc(Paths.CurrentProjectVirtualProjectsFolder);
            foreach (var name in paths)
            {
                var hasParse = VirtualProjectIdentifier.TryParse(name, out var identifier);
                var hasFiles = hasParse && HasRequiredDirectoriesForClone(fileSystemDelegates, identifier, out _);

                if (hasFiles)
                {
                    results.Add(identifier);
                }
            }

            return results.ToArray();
        }

        static bool HasRequiredDirectoriesForClone(FileSystemDelegates fileSystemDelegates, VirtualProjectIdentifier identifier, out string[] missingDirectories)
        {
            var projectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            var folderNames = fileSystemDelegates.GetDirectoryNamesFunc(projectPath);
            var resultMissingDirectories = new List<string>(PathsRequiredForProject);
            for (var index = resultMissingDirectories.Count - 1; index >= 0; index--)
            {
                var path = resultMissingDirectories[index];
                foreach (var info in folderNames)
                {
                    if (path == info)
                    {
                        resultMissingDirectories.Remove(path);
                    }
                }
            }

            // TODO Remove this when our minimum editor version is >=2023.3.0b8.
            // It's fine for the 'Temp' directory to be missing. Just create it in this case.
            if (resultMissingDirectories.Contains("Temp"))
            {
                var cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
                fileSystemDelegates.CreateDirectoryFunc(Path.Combine(cloneProjectPath, "Temp"));
                resultMissingDirectories.Remove("Temp");
            }

            missingDirectories = resultMissingDirectories.ToArray();
            return resultMissingDirectories.Count == 0;
        }

        public static void ManifestRegenerateForClones(ParsingSystemDelegates parsingSystemDelegates, FileSystemDelegates fileSystemDelegates, VirtualProjectIdentifier identifier)
        {
            var mainProjectPath = fileSystemDelegates.GetParentPathFunc(Paths.GetCurrentProjectDataPath());
            var mainProjectManifestPath = Path.Combine(mainProjectPath, "Packages", "manifest.json");

            var foldersInPackageFolder = fileSystemDelegates.GetDirectoryNamesFunc(Path.Combine(mainProjectPath, "Packages"));

            var cloneProjectPath = PathsUtility.GetProjectPathByIdentifier(identifier);
            var cloneManifestPath = Path.Combine(cloneProjectPath, "Packages", "manifest.json");

            var mainProjectManifest = fileSystemDelegates.ReadFileFunc(mainProjectManifestPath);
            var cloneProjectManifest = ManifestRewriteRelativePaths(parsingSystemDelegates, mainProjectManifest);
            cloneProjectManifest = ManifestAddCustomPackagesAsRelativePaths(parsingSystemDelegates, cloneProjectManifest, foldersInPackageFolder);

            fileSystemDelegates.WriteFileFunc(cloneManifestPath, cloneProjectManifest);
        }

        internal static string ManifestAddCustomPackagesAsRelativePaths(ParsingSystemDelegates parsingSystemDelegates, string manifestContent, string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return manifestContent;
            }

            var manifest = parsingSystemDelegates.ParseFunc(manifestContent);
            if (manifest.ContainsKey("dependencies"))
            {
                var dependencies = manifest["dependencies"].ToObject<Dictionary<string, string>>();

                // We just need to check for custom packages and then add the proper file dependencies to
                // the clones' manifests.
                foreach (var path in paths)
                {
                    var dirNameAsPackageName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
                    dependencies[dirNameAsPackageName] = "file:../../../../Packages/" + dirNameAsPackageName;
                }

                manifest["dependencies"] = parsingSystemDelegates.FromObjectFunc(dependencies);
            }

            return manifest.ToString(Formatting.Indented);
        }

        static string ManifestRewriteRelativePaths(ParsingSystemDelegates parsingSystemDelegates, string manifestContent)
        {
            var manifest = parsingSystemDelegates.ParseFunc(manifestContent);
            if (manifest.ContainsKey("dependencies"))
            {
                var dependencies = manifest["dependencies"].ToObject<Dictionary<string, string>>();
                if (dependencies != null)
                {
                    var keys = new List<string>(dependencies.Keys);

                    foreach (var package in keys)
                    {
                        if (dependencies[package].StartsWith("file:"))
                        {
                            var path = dependencies[package].Remove(0, "file:".Length).Replace("\\", "/");
                            if (!Path.IsPathRooted(path))
                            {
                                dependencies[package] = "file:../../../../Packages/" + path;
                            }
                        }
                    }

                    manifest["dependencies"] = parsingSystemDelegates.FromObjectFunc(dependencies);
                }
            }

            return manifest.ToString(Formatting.Indented);
        }
    }
}
