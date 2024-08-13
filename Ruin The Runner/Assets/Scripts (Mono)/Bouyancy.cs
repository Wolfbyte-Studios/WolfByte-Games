using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public float downwardVelocityThreshold = -0.1f; // Threshold to determine if the object is moving downwards
    public Vector3 offSet;
    public float rotationSpeed = 1.0f; // Speed at which the object rotates to align with the water surface

    public List<PlayerMovement.playertype> acceptedPlayers = new List<PlayerMovement.playertype>();
    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    // List to hold objects interacting with the water surface
    public Dictionary<GameObject, Quaternion> objectsToFit = new Dictionary<GameObject, Quaternion>();

    void FixedUpdate()
    {
        if (targetSurface != null)
        {
            if (objectsToFit == null) { return; }
            foreach (GameObject obj in objectsToFit.Keys)
            {
                Rigidbody rb = obj.transform.GetAllComponentsInHierarchy<Rigidbody>()[0];
                if (rb != null && rb.linearVelocity.y < downwardVelocityThreshold)
                {
                    // Build the search parameters
                    searchParameters.startPositionWS = obj.transform.position;
                    searchParameters.targetPositionWS = obj.transform.position;
                    searchParameters.error = 0.01f;
                    searchParameters.maxIterations = 8;

                    // Do the search
                    if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
                    {
                        if (obj.transform.position.y > searchResult.projectedPositionWS.y)
                        {
                            rb.linearDamping = 0.0f;
                            rb.angularDamping = 0.5f;
                            return;
                        }
                        rb.linearDamping = 1f;
                        rb.angularDamping = 1f;
                        obj.transform.position = (Vector3)searchResult.projectedPositionWS + offSet;
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                        // Calculate the desired rotation based on the water surface normal and the longest axis
                        Vector3 waterNormal = GetWaterSurfaceNormal(searchResult.projectedPositionWS);
                        obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, objectsToFit[obj], rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        Debug.LogError("Can't Find Projected Position for " + obj.name);
                    }
                }
            }
        }
    }

    // This method calculates the normal of the water surface at a given position
    Vector3 GetWaterSurfaceNormal(Vector3 position)
    {
        // Assuming the water surface is a simple plane for this example
        // For more complex surfaces, you will need to calculate the normal based on your surface data
        return Vector3.up;
    }

    // This method calculates the rotation to align the longest axis with the water surface normal
   

    // This method calculates the longest axis of the bounds
  

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.transform.GetAllComponentsInHierarchy<PlayerMovement>() != null)
        {
            var pM = other.gameObject.transform.GetAllComponentsInHierarchy<PlayerMovement>()[0];
            if (acceptedPlayers.Contains(pM.PlayerType))
            {
                pM.Respawn();
            }
        }
        if (objectsToFit == null)
        {
            var newGO = other.gameObject.transform.GetAllComponentsInHierarchy<Rigidbody>()[0].gameObject;
            if (other.gameObject.transform.GetComponent<DontFloat>() != null)
            { return; }

            objectsToFit.Add(newGO.gameObject, newGO.transform.rotation);
        }
        if (other.gameObject.transform.GetAllComponentsInHierarchy<Rigidbody>() != null)
        {
            if (other.gameObject.transform.GetComponent<DontFloat>() != null)
            { return; }
                var newGO = other.gameObject.transform.GetAllComponentsInHierarchy<Rigidbody>()[0].gameObject;
            if (!objectsToFit.ContainsKey(newGO.gameObject))
            {
               
                objectsToFit.Add(newGO.gameObject, newGO.transform.rotation);
            }
        }
    }

    

    void OnTriggerExit(Collider other)
    {
        if (objectsToFit.ContainsKey(other.gameObject))
        {
            objectsToFit.Remove(other.gameObject);
        }
    }
}
