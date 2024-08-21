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

        StepClimb(moveDirection);
    }

    void StepClimb(Vector3 direction)
    {
       
        var moving = pm.move.IsPressed();
        RaycastHit hitLower;
        Ray ray1 = new Ray(transform.position + new Vector3(0, lowerOffset.y, lowerOffset.x), direction);
        Ray ray2 = new Ray(transform.position + new Vector3(0, upperOffset.y, upperOffset.x), direction);
        Debug.DrawLine(ray1.origin, ray1.origin + ray1.direction * maxDistance1, Color.green);
        Debug.DrawLine(ray2.origin, ray2.origin + ray2.direction * maxDistance2, Color.red);
        
        if (Physics.Raycast(ray1, out hitLower, maxDistance1, layerMask, QueryTriggerInteraction.Ignore))
        {
            RaycastHit hitUpper;


            if (!Physics.Raycast(ray2, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (!moving)
                { return; }
                rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);
                var ray3 = new Ray(transform.position + new Vector3(0, upperOffset.y * 2, upperOffset.x), direction);
                Debug.DrawLine(ray3.origin, ray3.origin + ray3.direction * maxDistance2, Color.red);
                if (!Physics.Raycast(ray3, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (!moving)
                    { return; }
                    rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);

                    var ray4 = new Ray(transform.position + new Vector3(0, upperOffset.y * 3, upperOffset.x), direction);
                    Debug.DrawLine(ray4.origin, ray4.origin + ray4.direction * maxDistance2, Color.red);
                    if (!Physics.Raycast(ray4, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        if (!moving)
                        { return; }
                        rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);

                        var ray5 = new Ray(transform.position + new Vector3(0, upperOffset.y * 4, upperOffset.x), direction);
                        Debug.DrawLine(ray5.origin, ray5.origin + ray5.direction * maxDistance2, Color.red);
                        if (!Physics.Raycast(ray5, out hitUpper, maxDistance2, layerMask, QueryTriggerInteraction.Ignore))
                        {
                            if (!moving)
                            { return; }
                            rigidBody.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);


                        }
                        else
                        {
                            WallPushback(hitUpper, ray5);
                            return;
                            //rigidBody.linearVelocity = Physics.gravity;
                        }
                    }
                    else
                    {
                        WallPushback(hitUpper, ray4);
                        return;
                        //rigidBody.linearVelocity = Physics.gravity;
                    }
                }
                else
                {
                    WallPushback(hitUpper, ray3);
                    return;
                    //rigidBody.linearVelocity = Physics.gravity;
                }
               
            }
            else
            {
                WallPushback(hitUpper, ray2);
                return;
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
}
