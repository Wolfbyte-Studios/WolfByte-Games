using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public PlayerMovement pm;
    public float WaterHeightOffset;
    public float tolerance;
    public float projectedHeight;


    private Collider col;
    private bool InWater;

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    public void Start()
    {
        pm = transform.parent.GetComponent<PlayerMovement>();
        col = GetComponent<Collider>();
    }
    // Update is called once per frame
    void Update()
    {
        pm.anim.SetBool("InWater", InWater);
        pm.IsSwimming = InWater;
        if (InWater)
        {
            if (targetSurface != null)
            {
                // Build the search parameters
                searchParameters.startPositionWS = searchResult.candidateLocationWS;
                searchParameters.targetPositionWS = gameObject.transform.position;
                searchParameters.error = 0.01f;
                searchParameters.maxIterations = 8;
                projectedHeight = GetWaterSurfaceHeight();

                // Do the search
                if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
                {
                    // Clamp the y position of the player
                   /* gameObject.transform.position = new Vector3(
                        gameObject.transform.position.x,
                        Mathf.Clamp(gameObject.transform.position.y, targetSurface.volumeBounds.bounds.max.y * -1, targetSurface.transform.TransformPoint(targetSurface.volumeBounds.bounds.max).y + WaterHeightOffset),
                        gameObject.transform.position.z); */

                    // Check if the player is within the tolerance of the water surface
                    float distanceToWaterSurface = Mathf.Abs(gameObject.transform.position.y - targetSurface.volumeBounds.bounds.max.y);
                    if (distanceToWaterSurface <= tolerance)
                    {
                        pm.swimMaxHeight = true;
                        Debug.Log($"Within tolerance: {distanceToWaterSurface}");
                    }
                    else
                    {
                        pm.swimMaxHeight = false;
                        Debug.Log($"Outside tolerance: {distanceToWaterSurface}");
                    }
                }
                else
                {
                    Debug.LogError("Can't Find Projected Position");
                }
            }
        }
    }
    public float GetWaterSurfaceHeight()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = gameObject.transform.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                return searchResult.projectedPositionWS.y;
            }
            else
            {
                Debug.LogError("Can't Find Projected Position");
            }
        }

        return float.NaN; // Return NaN if the targetSurface is null or projection fails
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<WaterSurface>() != null)
        {
            InWater = true;
            pm.Gravity = Vector3.zero;
            pm.velocity = Vector3.zero;
            pm.anim.applyRootMotion = false;
            Debug.LogWarning("Velocity should be zero");
            targetSurface = other.GetComponent<WaterSurface>();
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<WaterSurface>() != null)
        {
            InWater = false;
            pm.anim.applyRootMotion = true;
            pm.Gravity = new Vector3(0, -9.81f, 0);
        }
    }
    
}