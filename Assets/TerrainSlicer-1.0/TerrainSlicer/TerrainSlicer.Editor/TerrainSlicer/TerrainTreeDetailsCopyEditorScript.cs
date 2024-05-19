using terrainslicer;
using UnityEditor;
using UnityEngine;

namespace TerrainSlicer
{
    [CustomEditor(typeof(TerrainTreeDetailsCopyController))]
    public class TerrainTreeDetailsCopyEditorScript : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var copyController = (TerrainTreeDetailsCopyController) target;

            EditorGUILayout.LabelField("Source terrain", "Choose source terrain to copy trees and/or details from");
            copyController.sourceTerrain =
                (Terrain) EditorGUILayout.ObjectField("Source", copyController.sourceTerrain, typeof(Terrain), true);

            copyController.copyTrees = EditorGUILayout.Toggle("Copy tree prototypes", copyController.copyTrees);
            copyController.copyDetails = EditorGUILayout.Toggle("Copy detail prototypes", copyController.copyDetails);

            if (copyController.sourceTerrain)
            {
                if (GUILayout.Button("Copy tree and/or detail prototypes", GUILayout.Height(32f)))
                {
                    copyController.PerformCopy();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}