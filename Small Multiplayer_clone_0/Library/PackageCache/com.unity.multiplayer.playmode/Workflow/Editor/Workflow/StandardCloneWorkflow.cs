#if UNITY_USE_MULTIPLAYER_ROLES
using Unity.Multiplayer.Editor;
#endif
using System.IO;
using System.Text;
using Unity.Multiplayer.Playmode.Common.Runtime;
using Unity.Multiplayer.Playmode.VirtualProjects.Editor;
using UnityEditor;

namespace Unity.Multiplayer.Playmode.Workflow.Editor
{
    class StandardCloneWorkflow
    {
        enum ScriptChangesDuringPlayOptions
        {
            RecompileAndContinuePlaying = 0,
            RecompileAfterFinishedPlaying = 1,
            StopPlayingAndRecompile = 2,
        }   // From ScriptChangesDuringPlayOptions in EditorApplication.cs

        const string k_ScriptCompilationDuringPlay = "ScriptCompilationDuringPlay";
        const string k_OpenGameViewOnPlay = "OpenGameViewOnEnteringPlayMode";

        static string s_LastWriteTime;

        public void Initialize(WorkflowCloneContext mppmContext, CloneContext vpContext)
        {
            CreateLayoutFiles();

            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.UIPollUpdate += () =>
            {
#if UNITY_USE_MULTIPLAYER_ROLES
                if(!EditorApplication.isPlaying)
                {
                    if (s_LastWriteTime != SystemDataStore.GetCloneLastWriteTime())
                    {
                        s_LastWriteTime = SystemDataStore.GetCloneLastWriteTime();
                        ///////////////
                        // We load in a new data store (and not overwrite the object)
                        // for now because we don't want to invalidate pointers.
                        // So we just read in the new values if they changed on the object that
                        // already exists in memory (across the whole system)
                        // The file could have been updated by MultiplayerWindow or by this clones TopView
                        // see :UpdatedDataStore for what this effects
                        var updatedDataStore = SystemDataStore.GetClone();
                        foreach (var (_,player) in updatedDataStore.LoadAllPlayerJson())
                        {
                            if (player.TypeDependentPlayerInfo.VirtualProjectIdentifier == VirtualProjectsEditor.CloneIdentifier)
                            {
                                var role = (MultiplayerRoleFlags)player.MultiplayerRole;
                                if (EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask != role)
                                {
                                    EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask = role;
                                    break;
                                }
                            }
                        }
                    }
                }
#endif
            };
            vpContext.CloneSystems.AssetImportEvents.RequestImport += (didDomainReload, numAssetsChanged) =>
            {
                // This prevents a race condition where the clone goes into playmode first and the main editor does a domain reload before it goes into playmode
                if (EditorPrefs.GetInt(k_ScriptCompilationDuringPlay) == (int)ScriptChangesDuringPlayOptions.StopPlayingAndRecompile && didDomainReload && numAssetsChanged > 0)
                {
                    MppmLog.Debug("MPPM Clone Stop Playing And Recompile");
#if UNITY_2023_2_OR_NEWER
                    ModeSwitcher.SaveCurrentView();
                    // Close the existing window if its open since we are about to reopen once we switch to view
                    // NOTE: We close the window AFTER saving the window layout!
                    ModeSwitcher.CloseCurrentWindow();
#endif
                    EditorApplication.ExitPlaymode();
                }
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerActive += () =>
            {
#if UNITY_MP_TOOLS_DEV_LOGMESSAGES
                // Do not update log counts when message logging is enabled,
                // otherwise logs will continually spam every time the message is sent
#else
#if UNITY_2023_2_OR_NEWER
                ConsoleWindowUtility.GetConsoleLogCounts(out var error, out var warning, out var log);
                vpContext.MessagingService.Broadcast(new UpdateCloneLogCountsMessage(VirtualProjectsEditor.CloneIdentifier, new LogCounts{Logs = log, Warnings = warning, Errors = error}));
#endif
#endif
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerTitleRename += editorTitleUpdater =>
            {
                const string unknownPlayerWindowTitle = "Unknown Player";
                var dataStore = SystemDataStore.GetClone();
                var hasPlayer = Filters.FindFirstPlayerWithVirtualProjectsIdentifier(dataStore.LoadAllPlayerJson(), VirtualProjectsEditor.CloneIdentifier, out var player);
                if (hasPlayer)
                {
                    var tag = player.Tags.Count <= 0 ? string.Empty : $" [{string.Join('|', player.Tags)}]";
                    editorTitleUpdater.title = $"{player.Name}{tag}";
                }
                else
                {
                    editorTitleUpdater.title = unknownPlayerWindowTitle;
                }
            };
            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.ConsoleLogMessagesChanged += () =>
            {
#if UNITY_MP_TOOLS_DEV_LOGMESSAGES
                    // Do not update log counts when message logging is enabled,
                    // otherwise logs will continually spam every time the message is sent
#else
                ConsoleWindowUtility.GetConsoleLogCounts(out var error, out var warning, out var log);
                vpContext.MessagingService.Broadcast(new UpdateCloneLogCountsMessage(VirtualProjectsEditor.CloneIdentifier, new LogCounts { Logs = log, Warnings = warning, Errors = error }));
#endif
            };

            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.PlayerPaused += () => vpContext.MessagingService.Broadcast(new PlayerPausedOnCloneMessage());

            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Play += () =>
            {
                /*
                 * The main editor keeps its functionality of starting on the scene tab and switching to game view when going into play mode (if OpenGameViewOnPlay is active)
                 * However, the clones always run with which ever view was selected from layout (since OpenGameViewOnPlay is always off)
                 *
                 * Edges cases:
                 * This is currently covering the main editor crashing (it uses the non deleted key in the local data store)
                 * The clone crashing (it just always runs with OpenGameViewOnPlay off)
                 * As well as these crashes happening in either order (main should be able to crash first or last and etc...)
                 *
                 * See MainCachePlayOnGameView for main editor behaviour
                 */

                // The following string and property needs to match the functionality
                //  of PlayModeView.openWindowOnEnteringPlayMode currently

                // We are a clone so we always go in as false.
                // The main editor will restore the previous mode
                EditorPrefs.SetBool(k_OpenGameViewOnPlay, false);

                // Save the current view first
#if UNITY_2023_2_OR_NEWER
                ModeSwitcher.SaveCurrentView();
                // Close the existing window if its open since we are about to reopen once we switch to view
                // NOTE: We close the window AFTER saving the window layout!
                ModeSwitcher.CloseCurrentWindow();
#endif
                EditorApplication.EnterPlaymode(); // intentionally after saving the view
            };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Pause += () => { EditorApplication.isPaused = true; };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Step += () => { EditorApplication.Step(); };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Unpause += () => { EditorApplication.isPaused = false; };
            mppmContext.ClonedPlayerSystems.PlaymodeEvents.Stop += () =>
            {
#if UNITY_2023_2_OR_NEWER
                ModeSwitcher.SaveCurrentView();
                // Close the existing window if its open since we are about to reopen once we switch to view
                // NOTE: We close the window AFTER saving the window layout!
                ModeSwitcher.CloseCurrentWindow();
#endif

                EditorApplication.ExitPlaymode(); // intentionally after saving the view
            };

            mppmContext.ClonedPlayerSystems.ClonedPlayerApplicationEvents.FrameAfterPlaymodeMessage += () =>
            {
                ModeSwitcher.SwitchToView(EditorApplication.isPlaying
                    ? CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).PlayModeLayoutFlags
                    : CloneDataFile.LoadFromFile(mppmContext.CloneDataFile).EditModeLayoutFlags);
            };
        }

        const string k_LayoutExtension = "sjson";
        static void CreateLayoutFiles()
        {
            // Start from 1 (not 0) on behalf of the enum flags
            for (var i = 1; i < (int)LayoutFlagsUtil.All + 1; i++)
            {
                // Transform flag into panels. Then panels into layout file. Then layout file to bytes.
                var flag = (LayoutFlags)i;
                var panelsData = PanelBuilder.BuildPanelsBasedOnFlags(flag);
                var layoutFile = LayoutFileBuilder.BuildLayoutFileBasedOnPanels(panelsData);
                var bytes = new UTF8Encoding(true).GetBytes(Convert.ToSJSON(layoutFile));

                var layoutFilePath = Path.Combine(ModeSwitcher.k_LayoutDirectory, $"{LayoutFlagsUtil.GenerateLayoutName(flag)}.{k_LayoutExtension}");
                var file = new FileInfo(layoutFilePath);
                file.Directory?.Create();
                File.WriteAllBytes(layoutFilePath, bytes);
            }
        }
    }
}
