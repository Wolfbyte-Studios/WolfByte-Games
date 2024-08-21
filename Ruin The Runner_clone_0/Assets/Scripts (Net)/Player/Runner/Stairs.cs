using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        pm = gameObject.GetComponent<PlayerMovement>();
        layerMask = pm.excludedLayers;
    }

    private void FixedUpdate()
    {
        var moveDirection = transform.TransformDirection(new Vector3(pm.move.ReadValue<Vector2>().x, 0, pm.move.ReadValue<Vector2>().y));

        
        if(pm.move.IsPressed() && StepClimb(moveDirection, upperOffset.y) && StepClimb(moveDirection, upperOffset.y*2 && StepClimb(moveDirection, upperOffset.y*3)) && StepClimb(moveDirection, upperOffset.y*4) && StepClimb(moveDirection, upperOffset.y*5))
        {       
            ClimbStep();
        }
    }

    public bool StepClimb(Vector3 direction, float YOffset)
    {
       
        var moving = pm.move.IsPressed();
        RaycastHit hitLower;
        Ray ray1 = new Ray(transform.position + new Vector3(0, lowerOffset.y, lowerOffset.x), direction);
        Ray ray2 = new Ray(transform.position + new Vector3(0, YOffset, upperOffset.x), direction);
        Debug.DrawLine(ray1.origin, ray1.origin + ray1.direction * maxDistance1, Color.green);
        Debug.DrawLine(ray2.origin, ray2.origin + ray2.direction * maxDistance2, Color.red);
        
        if (Physics.Raycast(ray1, out hitLower, maxDistance1, layerMask, QueryTriggerInteraction.Ignore))
        {
            RaycastHit hitUpper;


            if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
            {
                
                return true;
            }
            else
            {
                WallPushback(hitUpper, ray2);
                return false;
                //rigidBody.linearVelocity = Physics.gravity;
            }
        }

    }
    public void WallPushback(RaycastHit hit, Ray ray)
    {
        rigidBody.position = rigidBody.position - ((hit.point - ray.origin) * bounceBackMultiplier);
        pm.velocity = new Vector3(0, pm.velocity.y, 0) + Physics.gravity * bounceBackMultiplier;
        rigidBody.linearVelocity = pm.velocity;
    }
    public void ClimbStep()
    {
        if (!moving)
        { return; }
        rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);
    }
}
