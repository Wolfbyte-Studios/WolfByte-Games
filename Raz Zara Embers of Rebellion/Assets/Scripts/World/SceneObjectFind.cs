using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

public class SceneObjectFinder : MonoBehaviour
{
    public string sceneToLoad; // Changed from SceneAsset to string to avoid editor-only dependency
    public int selectedSpawnPointIndex = 0; // Changed from string to int
    public List<string> spawnPoints = new List<string>();

    public void LoadSceneAndParse()
    {
#if UNITY_EDITOR
        SceneObjectFinderEditorHelper.LoadSceneAndParseEditor(this);
#else
        Debug.LogError("This function should only be called in the editor.");
#endif
    }
}

#if UNITY_EDITOR
public static class SceneObjectFinderEditorHelper
{
    public static void LoadSceneAndParseEditor(SceneObjectFinder finder)
    {
        // Clear the previous list of spawn points
        finder.spawnPoints.Clear();

        // Find the scene asset path by its name
        string[] guids = AssetDatabase.FindAssets(finder.sceneToLoad + " t:Scene");
        if (guids.Length == 0)
        {
            Debug.LogError("Scene not found: " + finder.sceneToLoad);
            return;
        }

        string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Scene path is empty for scene: " + finder.sceneToLoad);
            return;
        }

        // Temporarily load the scene
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        // Parse the scene
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            foreach (Transform t in obj.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("ExitEntry")) // Replace with your tag or condition
                {
                    finder.spawnPoints.Add(t.name);
                }
            }
        }

        // Unload the scene
        EditorSceneManager.CloseScene(scene, true);
    }
}
#endif
