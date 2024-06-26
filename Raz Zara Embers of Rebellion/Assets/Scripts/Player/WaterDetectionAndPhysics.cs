using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterDetectionAndFit : MonoBehaviour
{
    [Header("Water Detection")]
    public LayerMask waterLayer; // Assign the water layer in the inspector
    public float waterSurfaceDetectionDistance = 1f; // Distance to check for water surface
    public float underwaterDetectionDistance = 2f; // Distance to check if underwater
    public bool isOnWaterSurface;
    public bool isUnderwater;

    [Header("Water Surface Fitting")]
    public WaterSurface targetSurface = null;

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    void Update()
    {
        CheckIfOnWaterSurface();
        CheckIfUnderwater();
        FitToWaterSurface();
    }

    void CheckIfOnWaterSurface()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = transform.GetChild(0).position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                float distanceToSurface = Vector3.Distance(transform.GetChild(0).position, searchResult.projectedPositionWS);

                if (distanceToSurface <= waterSurfaceDetectionDistance)
                {
                    isOnWaterSurface = true;
                    isUnderwater = false;
                    Debug.Log("Character is on the water surface.");
                }
                else
                {
                    isOnWaterSurface = false;
                }
            }
            else
            {
                isOnWaterSurface = false;
            }
        }
    }

    void CheckIfUnderwater()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = transform.GetChild(0).position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                float distanceToSurface = Vector3.Distance(transform.GetChild(0).position, searchResult.projectedPositionWS);

                if (!isOnWaterSurface && distanceToSurface <= underwaterDetectionDistance)
                {
                    isUnderwater = true;
                    Debug.Log("Character is underwater.");
                }
                else
                {
                    isUnderwater = false;
                }
            }
            else
            {
                isUnderwater = false;
            }
        }
    }

    void FitToWaterSurface()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = transform.GetChild(0).position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                Debug.Log(searchResult.projectedPositionWS);
                transform.GetChild(0).position = searchResult.projectedPositionWS;
            }
            else
            {
                Debug.LogError("Can't Find Projected Position");
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a ray in the scene view for debugging purposes
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.GetChild(0).position, Vector3.down * waterSurfaceDetectionDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.GetChild(0).position, Vector3.up * underwaterDetectionDistance);
    }
}
