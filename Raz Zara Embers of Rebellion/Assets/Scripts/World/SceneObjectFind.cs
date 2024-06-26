using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneObjectFinder : MonoBehaviour
{
    public SceneAsset sceneToLoad;
    public int selectedSpawnPointIndex = 0; // Change from string to int
    public List<string> spawnPoints = new List<string>();

    public void LoadSceneAndParse()
    {
#if UNITY_EDITOR
        // Clear the previous list of spawn points
        spawnPoints.Clear();
        // Temporarily load the scene
        Scene scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath( sceneToLoad), OpenSceneMode.Additive);

        // Parse the scene
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            foreach (Transform t in obj.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("ExitEntry")) // Replace with your tag or condition
                {
                    spawnPoints.Add(t.name);
                }
            }
        }

        // Unload the scene
        EditorSceneManager.CloseScene(scene, true);
#else
        Debug.LogError("This function should only be called in the editor.");
#endif
    }
}
