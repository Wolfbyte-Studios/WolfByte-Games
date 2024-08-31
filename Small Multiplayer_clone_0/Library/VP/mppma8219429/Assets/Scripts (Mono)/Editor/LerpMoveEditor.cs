using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LerpMovement))]
public class LerpMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LerpMovement script = (LerpMovement)target;
        if (GUILayout.Button("Add Current Transform"))
        {
            Undo.RecordObject(script, "Add Transform");
            script.transforms.Add(new LerpMovement.TransformData
            {
                position = script.transform.position,
                rotation = script.transform.rotation,
                speed = 1
            });
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Trigger Movement"))
        {
            script.Trigger();
        }
    }
}
