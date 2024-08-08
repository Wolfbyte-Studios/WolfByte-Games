using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rigidbody))]
public class CenterOfMassVisualizer : Editor
{
    void OnSceneGUI()
    {
        // Get the selected object
        Rigidbody rb = (Rigidbody)target;

        // Draw a sphere at the center of mass
        Handles.color = Color.red;
        Handles.SphereHandleCap(0, rb.worldCenterOfMass, Quaternion.identity, 0.2f, EventType.Repaint);

        // Optionally, draw a line from the object's position to the center of mass
        Handles.color = Color.yellow;
        Handles.DrawLine(rb.transform.position, rb.worldCenterOfMass);
    }
}
