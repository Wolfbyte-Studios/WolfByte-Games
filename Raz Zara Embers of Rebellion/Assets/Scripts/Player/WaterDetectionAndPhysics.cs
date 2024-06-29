using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TextCore.Text;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public PlayerMovement pm;
    public float WaterHeightOffset;
    public float tolerance;
    public float projectedHeight;
    public float distanceToWaterSurface;

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
        
        
        if (InWater)
        {
            CheckIfCharacterIsInTrigger();
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
                    distanceToWaterSurface = Mathf.Abs(gameObject.transform.position.y - targetSurface.volumeBounds.bounds.max.y);
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
                    if(distanceToWaterSurface <= tolerance)
                    {
                        pm.cc.Move(new Vector3(0, -0.1f, 0));
                    }
                }
                else
                {
                    Debug.LogError("Can't Find Projected Position");
                }
            }
            
        }
    }
    public Collider triggerCollider; 
    public Transform character;
    void CheckIfCharacterIsInTrigger()
    {
        if(triggerCollider == null)
        {
            return;
        }
        // Get the bounds of the trigger collider
        Bounds triggerBounds = triggerCollider.bounds;
        
        // Check if the character's position is within the bounds
        if (triggerBounds.Contains(transform.position))
        {
            //Debug.Log("Character is within the trigger bounds.");
            // Handle in-trigger logic here
        }
        else
        {
            OnTriggerExit(triggerCollider);
            triggerCollider = null;
            return;
            // Handle out-of-trigger logic here
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
            triggerCollider = other;
            InWater = true;
            pm.anim.SetBool("InWater", InWater);
            pm.IsSwimming = InWater;
            pm.Gravity = Vector3.zero;
            pm.velocity = Vector3.zero;
            pm.anim.applyRootMotion = false;
            pm.anim.SetBool("IsFalling", false);
            pm.anim.SetBool("IsGrounded", true);
            Debug.LogWarning("Velocity should be zero");
            targetSurface = other.GetComponent<WaterSurface>();
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<WaterSurface>() != null)
        {
            InWater = false;
            pm.anim.SetBool("InWater", InWater);
            pm.IsSwimming = InWater;
            pm.anim.applyRootMotion = true;
            pm.Gravity = new Vector3(0, -9.81f, 0);
        }
    }
    
}