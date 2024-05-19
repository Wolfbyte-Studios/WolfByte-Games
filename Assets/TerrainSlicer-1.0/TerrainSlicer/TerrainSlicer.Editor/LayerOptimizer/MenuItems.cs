using layercleanup;
using UnityEditor;
using UnityEngine;

namespace LayerOptimizer
{
    [ExecuteInEditMode]
    public class MenuItems : MonoBehaviour
    {
        [MenuItem("Window/Terrain Slicer/Add Layer Operations Component")]
        private static void DecorateTerrainWithLayerOperationsComponent()
        {
            var selection = Selection.activeGameObject;
            if (selection == null || selection.GetComponent<Terrain>() == null)
            {
                EditorUtility.DisplayDialog("Warning", "Please, select terrain where to add LayerOperations script", "OK");
                return;
            }

            if (selection.GetComponent<TerrainLayerUsageOperations>() == null)
            {
                selection.AddComponent<TerrainLayerUsageOperations>();
            }
            else
            {
                Debug.Log($"{nameof(TerrainLayerUsageOperations)} already added to {selection.name}");
            }
        }
    }
}