using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public PlayerMovement pm;

    private bool InWater;

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    public void Start()
    {
        pm = transform.parent.GetComponent<PlayerMovement>();
    }
    // Update is called once per frame
    void Update()
    {
        pm.anim.SetBool("InWater", InWater);
        pm.IsSwimming = InWater;
        if (InWater)
        {

            
        }
        /*For Later
         * if (targetSurface != null) 
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = gameObject.transform.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                Debug.Log(searchResult.projectedPositionWS);
                gameObject.transform.position = searchResult.projectedPositionWS;
            }
            else Debug.LogError("Can't Find Projected Position");
        }
    */

    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<WaterSurface>() != null)
        {
            InWater = true;
            pm.Gravity = Vector3.zero;
            pm.velocity = Vector3.zero;
            Debug.LogWarning("Velocity should be zero");
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