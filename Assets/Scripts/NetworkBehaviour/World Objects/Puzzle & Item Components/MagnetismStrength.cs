using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MagnetismStrength : MonoBehaviour
{
    public float strength = 1f;  // Strength of magnetism
    public bool isActive = false;  // Whether magnetism is active
    public Rigidbody rb;
    public Collider col;
    public float effectiveRange = 10f;  // Effective range of magnetic interaction
    public Vector3 extents;
    public WorldPropertiesController wp;


    public enum Polarities {
        North,
        South
    }
    public Polarities Polarity;
    void Start()
    {
       
        wp = GameObject.Find("WorldPropertiesController").GetComponent<WorldPropertiesController>();
        rb = GetComponent<Rigidbody>();
        if(Random.value < 0.5)
        {
            Polarity = Polarities.North;
        }
        else
        {
            Polarity = Polarities.South;
        }
    }

    void FixedUpdate()
    {
        if (isActive )
        {
            ApplyMagneticForces();
        }

    }
    public void FlipPolarity()
    {
        Debug.Log("Holy Fuck");
        switch (Polarity)
        {
            case Polarities.North:
                Polarity = Polarities.South;
                break;
            case Polarities.South:
                Polarity = Polarities.North;
                break;
        }
    }
    private void OnDestroy()
    {
        wp.inRadius.Remove(gameObject);
    }

    private void ApplyMagneticForces()
    {


        
        //Add poles
        
        // Get all magnetic objects
        MagnetismStrength[] magnets = FindObjectsByType<MagnetismStrength>(FindObjectsSortMode.None);
        foreach (MagnetismStrength magnet in magnets)
        {
            if (magnet.isActive && magnet != this)
            {
                
                Vector3 forceDirection = magnet.transform.position - transform.position;
                if (magnet.Polarity == Polarity)
                {
                    forceDirection = -forceDirection;
                }
                float distanceSquared = forceDirection.sqrMagnitude;
                float mass = magnet.gameObject.GetComponent<Rigidbody>().mass;
                // Apply forces only if within effective range
                if (distanceSquared < effectiveRange * effectiveRange && distanceSquared > 0.01f)  // Prevent extreme forces at very small distances
                {
                    forceDirection.Normalize();
                    float forceMagnitude = (strength * magnet.strength * rb.mass * mass) / distanceSquared;
                    rb.AddForce(forceDirection * forceMagnitude);
                }
            }
        }
    }

   
}
