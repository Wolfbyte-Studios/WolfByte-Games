using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformExtension))]
[CanEditMultipleObjects]
public class TransformExtensionEditor : UnityEditor.Editor
{
    SerializedProperty targetLength;
    SerializedProperty snapValue;
    SerializedProperty snappingEnabled;

    void OnEnable()
    {
        targetLength = serializedObject.FindProperty("targetLength");
        snapValue = serializedObject.FindProperty("snapValue");
        snappingEnabled = serializedObject.FindProperty("snappingEnabled");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TransformExtension transformExtension = (TransformExtension)target;

        // Display default Transform properties
        DrawDefaultInspector();

        // Draw custom Length field
        EditorGUILayout.PropertyField(targetLength, new GUIContent("Length"));

        // Draw snap value and snapping toggle
        EditorGUILayout.PropertyField(snapValue, new GUIContent("Snap Value"));
        EditorGUILayout.PropertyField(snappingEnabled, new GUIContent("Enable Snapping"));

        if (GUI.changed)
        {
            transformExtension.AdjustScale();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
