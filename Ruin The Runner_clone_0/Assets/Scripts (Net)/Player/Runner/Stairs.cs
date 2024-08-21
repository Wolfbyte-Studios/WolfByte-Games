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
       

            StepClimb();
        
    }

    void StepClimb()
    {
        RaycastHit hitLower;
        Ray ray1 = new Ray(transform.position + new Vector3(0, lowerOffset.y, lowerOffset.x), transform.forward);
        Ray ray2 = new Ray(transform.position + new Vector3(0, upperOffset.y, upperOffset.x), transform.forward);
        Debug.DrawLine(ray1.origin, ray1.origin + ray1.direction * maxDistance1, Color.green);
        Debug.DrawLine(ray2.origin, ray2.origin + ray2.direction * maxDistance2, Color.red);
        
        if (Physics.Raycast(ray1, out hitLower, maxDistance1, layerMask, QueryTriggerInteraction.Ignore))
        {
            RaycastHit hitUpper;


            if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (!pm.move.IsPressed())
                { return; }
                rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);
                ray2.origin = transform.position + new Vector3(0, upperOffset.y * 2, upperOffset.x);
                if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (!pm.move.IsPressed())
                    { return; }
                    rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);


                }
                else
                {
                    WallPushback(hitUpper, ray2);
                    //rigidBody.linearVelocity = Physics.gravity;
                }
                ray2.origin = transform.position + new Vector3(0, upperOffset.y * 3, upperOffset.x);
                if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (!pm.move.IsPressed())
                    { return; }
                    rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);

                    ray2.origin = transform.position + new Vector3(0, upperOffset.y * 4, upperOffset.x);
                    if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        if (!pm.move.IsPressed())
                        { return; }
                        rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);


                    }
                    else
                    {
                        WallPushback(hitUpper, ray2);
                        //rigidBody.linearVelocity = Physics.gravity;
                    }
                }
                else
                {
                    WallPushback(hitUpper, ray2);
                    //rigidBody.linearVelocity = Physics.gravity;
                }
            }
            else
            {
                WallPushback(hitUpper, ray2);
                //rigidBody.linearVelocity = Physics.gravity;
            }
        }

    }
    public void WallPushback(RaycastHit hit, Ray ray)
    {
        rigidBody.position = rigidBody.position - ((hit.point - ray.origin) * bounceBackMultiplier);
        pm.velocity = Vector3.zero + Physics.gravity;
        rigidBody.linearVelocity = pm.velocity;
    }
}
