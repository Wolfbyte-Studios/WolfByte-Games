using terrainslicer;
using UnityEditor;
using UnityEngine;

namespace TerrainSlicer
{
    public class MenuItems : MonoBehaviour
    {
        [MenuItem("Window/Terrain Slicer/Add Terrain Slicer Component")]
        private static void DecorateTerrainWithSlicerComponent()
        {
            var selection = Selection.activeGameObject;
            if (selection == null || selection.GetComponent<Terrain>() == null)
            {
                EditorUtility.DisplayDialog("Warning", "Please, select terrain where to add Slicer script", "OK");
                return;
            }

            if (selection.GetComponent<TerrainSlicerController>() == null)
            {
                selection.AddComponent<TerrainSlicerController>();
            }
            else
            {
                Debug.Log($"{nameof(TerrainSlicerController)} already added to {selection.name}");
            }
        }
        
        [MenuItem("Window/Terrain Slicer/Add Tree or Details Copy Component")]
        static void DecorateTerrainWithTreeDetailsCopyComponent()
        {
            var selection = Selection.activeGameObject;
            if (selection == null || selection.GetComponent<Terrain>() == null)
            {
                EditorUtility.DisplayDialog("Warning", "Please, select terrain where to add Copy script", "OK");
                return;
            }

            if (selection.GetComponent<TerrainTreeDetailsCopyController>() == null)
            {
                selection.AddComponent<TerrainTreeDetailsCopyController>();
            }
            else
            {
                Debug.Log($"{nameof(TerrainTreeDetailsCopyController)} already added to {selection.name}");
            }
        }
    }
}