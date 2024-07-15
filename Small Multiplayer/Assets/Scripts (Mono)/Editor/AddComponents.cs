using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using UnityEngine.Events;

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
                Clickable c = null;
                LerpMovement l = null;
                // Add Clickable component if it doesn't already exist
                if (obj.GetComponent<Clickable>() == null)
                {
                   c = obj.AddComponent<Clickable>();
                }
                else
                {
                    c = obj.GetComponent<Clickable>();
                }
                if (obj.GetComponent<NetworkObject>() == null)
                {
                    obj.AddComponent<NetworkObject>();
                }
                if (obj.GetComponent<LerpMovement>() == null)
                {
                    l = obj.AddComponent<LerpMovement>();
                }
                Material mat = null;
                if(obj.GetComponent<MeshRenderer>() != null)
                {
                    mat = obj.GetComponent<MeshRenderer>().sharedMaterial = new Material(obj.GetComponent<MeshRenderer>().sharedMaterial.shader);
                }
                else
                {
                    mat = obj.transform.FindDeepChildrenByType<MeshRenderer>()[0].GetComponent<MeshRenderer>().sharedMaterial = new Material(obj.transform.FindDeepChildrenByType<MeshRenderer>()[0].GetComponent<MeshRenderer>().sharedMaterial.shader);
                }
                
                mat.color = Color.magenta;
                obj.tag = "Clickable";
                obj.layer = 30;
                c.low = Color.red;
                c.med = Color.yellow;
                c.high = Color.green;
                c.done = Color.white;
                
                if(obj.GetComponent<MeshCollider>() != null)
                {
                    if (l = null) { return; }
                    if (obj.GetComponent<LerpMovement>().rigBody == true)
                    {
                        obj.GetComponent<MeshCollider>().convex = true;
                    }
                    else
                    {
                        obj.GetComponent<MeshCollider>().convex = false;
                    }
                }

               
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

        //Debug.Log("Components added based on object names.");
    }
}
