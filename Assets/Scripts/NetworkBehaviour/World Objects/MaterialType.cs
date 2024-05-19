using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MaterialType : MonoBehaviour
{
    public enum Material
    {
        Wood,
        Metal,
        Plastic,
        Danger1,
        Danger2,
        Danger3,
        Death,
        Default,
        Rock,
        Glass
    }
    public enum MaterialProperty
    {
        Flammable,
        Magnetic,
        Collectable,
        Pushable,
        Interactable,
        Breakable,
        Diggable,
        Painful1,
        Painful2,
        Painful3,
        Death

    }
    public float baseMass;
    public WorldPropertiesController worldPropertiesController;

    // Dropdown selector in the inspector
    [Tooltip("Select what type of material this is:")]
    public Material materialType;
    [Tooltip("Any additional properties?")]
    public List<MaterialProperty> properties = new List<MaterialProperty>();

    private Rigidbody rb;
    public MeshRenderer mr;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateMaterialDensity();
        if(GetComponent<MeshRenderer>() == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }
        else
        {
            mr = GetComponent<MeshRenderer>();
        }
        
    }

    // Update the density whenever the material type is changed in the inspector
    void OnValidate()
    {
        UpdateMaterialDensity();
    }
    private void FixedUpdate()
    {
        rb.mass = gameObject.GetComponent<Collider>().bounds.extents.x * gameObject.GetComponent<Collider>().bounds.extents.y * gameObject.GetComponent<Collider>().bounds.extents.z * baseMass * 10;
    }

    // Method to update the Rigidbody's mass based on the material type
    void UpdateMaterialDensity()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (mr == null)
        {
            mr = GetComponent<MeshRenderer>();
        }
        if(worldPropertiesController == null)
        {
            worldPropertiesController = GameObject.Find("WorldPropertiesController").GetComponent<WorldPropertiesController>();
        }

        switch (materialType)
        {
            case Material.Default:
                baseMass = 1.2f;  // Density for wood, relatively light
                break;
            case Material.Wood:
                baseMass = 1.2f;
                mr.material = worldPropertiesController.Wood;// Density for wood, relatively light
                break;
            case Material.Metal:
                mr.material = worldPropertiesController.Metal;
                baseMass = 7.8f;  // Density for metal, much heavier
                break;
            case Material.Plastic:
                mr.material = worldPropertiesController.Plastic;
                baseMass = 0.9f;  // Density for plastic, lighter than metal
                break;
            case Material.Danger1:
                mr.material = worldPropertiesController.Danger1;
                baseMass = 10;
                break;
            case Material.Danger2:
                mr.material = worldPropertiesController.Danger2;
                baseMass = 10;
                break;
            case Material.Danger3:
                mr.material = worldPropertiesController.Danger3;
                baseMass = 10;
                break;
            case Material.Death:
                mr.material = worldPropertiesController.Death;
                baseMass = 10;
                break;
            default:
                Debug.LogError("Unknown material type!");
                break;
            case Material.Rock:
                mr.material = worldPropertiesController.Rock;
                baseMass = 5f;
                break;
            case Material.Glass:
                mr.material = worldPropertiesController.Glass;
                baseMass = 5f;
                break;
        }
        if (properties.Contains(MaterialProperty.Magnetic) == true && (gameObject.GetComponent<MagnetismStrength>() == null))
        {
            gameObject.AddComponent<MagnetismStrength>();
        }
        if (properties.Contains(MaterialProperty.Pushable) == true && (gameObject.GetComponent<Pushable>() == null))
        {
            gameObject.AddComponent<Pushable>();
        }
        rb.mass = gameObject.GetComponent<Collider>().bounds.extents.x * gameObject.GetComponent<Collider>().bounds.extents.y * gameObject.GetComponent<Collider>().bounds.extents.z * baseMass * 10;
    }
}
