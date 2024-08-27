using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StairClimb : MonoBehaviour
{
    Rigidbody rigidBody;
    public Vector2 lowerOffset;
    public Vector2 upperOffset;
    public float maxDistance1;
    public float maxDistance2;
    LayerMask layerMask;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = .1f;
    public float bounceBackMultiplier = 1f;
    PlayerMovement pm;
    public List<string> AcceptedTagsForClimb = new List<string>() { "Ladder", "Climbable"};
    string tagOfPotentialClimbable;
    public float distanceToStair;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        pm = gameObject.GetComponent<PlayerMovement>();
        layerMask = pm.excludedLayers;
    }

    private void FixedUpdate()
    {
        // Calculate the initial moveDirection based on the input
        Vector3 moveDirection = transform.TransformDirection(new Vector3(pm.move.ReadValue<Vector2>().x, 0, pm.move.ReadValue<Vector2>().y));

        // Calculate the right diagonal direction (45 degrees to the right of moveDirection)
        Vector3 rightDiagonal = Quaternion.Euler(0, 45, 0) * moveDirection;

        // Calculate the left diagonal direction (45 degrees to the left of moveDirection)
        Vector3 leftDiagonal = Quaternion.Euler(0, -45, 0) * moveDirection;



        if (CheckRays(moveDirection) || CheckRays(rightDiagonal) || CheckRays(leftDiagonal))
        {
            ClimbStep();
        }
        
    }
    public bool CheckRays(Vector3 moveDirection)
    {
        if (StepClimb(moveDirection, upperOffset.y) && StepClimb(moveDirection, upperOffset.y * 2) && StepClimb(moveDirection, upperOffset.y * 3) && StepClimb(moveDirection, upperOffset.y * 4) && StepClimb(moveDirection, upperOffset.y * 5))
        {
            return true;
        }
        else
        {

            
            return false;
        }
    }
    public float stepUpForwardCheck = .1f;
    public Vector3 newFootPlacement;
    public float heightOffset = .15f;

    public bool StepClimb(Vector3 direction, float YOffset)
    {
        tagOfPotentialClimbable = "";
       
        var moving = pm.move.IsPressed();
        RaycastHit hitLower;
        Ray ray1 = new Ray(new Vector3(((pm.leftFoot.position.x + pm.rightFoot.position.x) / 2), transform.position.y + lowerOffset.y, ((pm.leftFoot.position.z + pm.rightFoot.position.z) / 2)), direction);
        Ray ray2 = new Ray(new Vector3(((pm.leftKnee.position.x + pm.rightKnee.position.x) / 2), ((pm.leftKnee.position.y + pm.rightKnee.position.y) / 2), ((pm.leftKnee.position.z + pm.rightKnee.position.z) / 2)), direction);
       
        Debug.DrawLine(ray1.origin, ray1.origin + ray1.direction * maxDistance1, Color.green);
        Debug.DrawLine(ray2.origin, ray2.origin + ray2.direction * maxDistance2, Color.red);
        
        if (Physics.Raycast(ray1, out hitLower, maxDistance1, layerMask, QueryTriggerInteraction.Ignore))
        {
           
           
            RaycastHit hitUpper;
            if(AcceptedTagsForClimb.Contains(hitLower.collider.gameObject.tag))
            {
                tagOfPotentialClimbable = hitLower.collider.gameObject.tag;
                Debug.Log(tagOfPotentialClimbable);
                distanceToStair = hitLower.distance / maxDistance1;
                return true;
            }

            if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 orig = new Vector3(hitLower.point.x, ray2.origin.y, hitLower.point.z) + (ray1.direction * stepUpForwardCheck);
                Ray downRay = new Ray(orig, Vector3.down);
                Debug.DrawLine(downRay.origin, downRay.origin + downRay.direction * maxDistance2, Color.blue);
                RaycastHit downhit;
                if(Physics.Raycast(downRay, out downhit, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                {
                    newFootPlacement = downhit.point + (Vector3.up * heightOffset) ;
                    Debug.Log("New Foot Placement: " + newFootPlacement + downhit.point);
                    return true;
                }
                /*distanceToStair = hitLower.distance / maxDistance2;
                return true;*/
            }
            else
            {
                if (AcceptedTagsForClimb.Contains(hitUpper.collider.gameObject.tag))
                {
                    tagOfPotentialClimbable = hitLower.collider.gameObject.tag;
                    Debug.Log(tagOfPotentialClimbable);
                    return true;
                }
                WallPushback(hitUpper, ray2);
                return false;
                //rigidBody.linearVelocity = Physics.gravity;
            }
        }

        pm.anim.SetBool("Climbing", false);
        return false;
    }
    public void WallPushback(RaycastHit hit, Ray ray)
    {
        rigidBody.position = rigidBody.position - ((hit.point - ray.origin) * bounceBackMultiplier);

        pm.anim.SetBool("Climbing", false);
        pm.velocity = new Vector3(0, pm.velocity.y, 0) + (Physics.gravity * bounceBackMultiplier);
        rigidBody.linearVelocity = pm.velocity;
    }
    public float ladderScale = 2;
    public void ClimbStep()
    {
        if (pm.move.IsPressed())
        {
            var stepAmount = stepSmooth;
            if (tagOfPotentialClimbable == "Ladder" || tagOfPotentialClimbable == "Climbable")
            {
                
                rigidBody.position += new Vector3(0f, (ladderScale * Time.deltaTime), 0f);
                pm.anim.SetBool("Climbing", true);
            }
            else
            {
               
                rigidBody.position = Vector3.Lerp(rigidBody.position, newFootPlacement, stepSmooth);
                rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.z, 0, rigidBody.linearVelocity.z);
            }
            
        }
        else
        {
            pm.anim.SetBool("Climbing", false);
        }
    }
}
