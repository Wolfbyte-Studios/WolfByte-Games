using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Clickable))]
public class ClickableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Clickable clickable = (Clickable)target;
        if (GUILayout.Button("Trigger Event"))
        {
            clickable.TriggerEvent();
        }
    }
}
