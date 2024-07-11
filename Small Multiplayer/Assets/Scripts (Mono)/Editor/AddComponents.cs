using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

public class AddComponentsBasedOnName
{
    [MenuItem("Tools/Add Components Based on Name")]
    private static void AddComponents()
    {
        // Get all objects in the scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Loop through each object
        foreach (GameObject obj in allObjects)
        {
            // Check if the object name contains "Clickable"
            if (obj.name.Contains("Clickable"))
            {
                // Add Clickable component if it doesn't already exist
                if (obj.GetComponent<Clickable>() == null)
                {
                    obj.AddComponent<Clickable>();
                }
                if (obj.GetComponent<NetworkObject>() == null)
                {
                    obj.AddComponent<NetworkObject>();
                }

                obj.tag = "Clickable";
                obj.layer = 30;
            }
            if (obj.name.Contains("Water"))
            {
                // Add Clickable component if it doesn't already exist
                if (obj.GetComponent<WaterSurface>() == null)
                {
                    obj.AddComponent<WaterSurface>();
                }
                var water = obj.GetComponent<WaterSurface>();
                water.surfaceType = WaterSurfaceType.Pool;
                water.geometryType = WaterGeometryType.Custom;
                water.meshRenderers.Add( obj.GetComponent<MeshRenderer>());
                obj.GetComponent<MeshRenderer>().enabled = false;

            }

            // You can add more conditions here for other components
            // For example, if an object name contains "Movable", add a Movable component

        }

        Debug.Log("Components added based on object names.");
    }
}
