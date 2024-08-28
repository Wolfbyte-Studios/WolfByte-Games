using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    static string[] GetScenes()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/Windows/MyGame.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Windows build completed.");
    }

    [MenuItem("Build/Build Mac")]
    public static void BuildMac()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/Mac/MyGame.app", BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("Mac build completed.");
    }

    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "Builds/Android/MyGame.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log("Android build completed.");
    }
}
