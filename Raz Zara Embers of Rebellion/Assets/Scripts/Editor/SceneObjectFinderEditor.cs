using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneObjectFinder))]
public class SceneObjectFinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneObjectFinder finder = (SceneObjectFinder)target;

        if (GUILayout.Button("Load Scene and Parse"))
        {
            finder.LoadSceneAndParse();
        }

        if (finder.spawnPoints.Count > 0)
        {
            EditorGUILayout.LabelField("Select Spawn Point:");
            finder.selectedSpawnPointIndex = EditorGUILayout.Popup(finder.selectedSpawnPointIndex, finder.spawnPoints.ToArray());
        }
    }
}
