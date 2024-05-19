using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPropertiesController : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> inRadius = new List<GameObject>();
    public Material Metal;
    public Material Wood;
    public Material Plastic;
    public Material Danger1;
    public Material Danger2;
    public Material Danger3;
    public Material Death;
    public Material Rock;
    public Material Glass;
    void Start()
    {
        
    }
    private void OnValidate()
    {
        var RBS = GameObject.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach(Rigidbody rb in RBS)
        {
            if (rb.gameObject.GetComponent<MaterialType>() == null)
            {
                rb.gameObject.AddComponent<MaterialType>();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject obj in inRadius)
        {
            var material = obj.GetComponent<MaterialType>();
            
            if (material.properties.Contains(MaterialType.MaterialProperty.Flammable))
            {
                //Flammable Logic Here
            }
            



        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<MaterialType>() != null)
        {
            Activate(true, other.gameObject);
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
       if( inRadius.Contains(other.gameObject) )
        {
           Activate(false, other.gameObject);
           
        }
    }

    public void Activate(bool nowActive, GameObject other)
    {
        if (nowActive)
        {

            inRadius.Add(other.gameObject);
        }
        if (other.gameObject.GetComponent<MaterialType>().properties.Contains(MaterialType.MaterialProperty.Magnetic))
        {
            other.GetComponent<MagnetismStrength>().isActive = nowActive;
        }
        if (!nowActive)
        {
            inRadius.Remove(other.gameObject);
        }
    }
}
