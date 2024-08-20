using UnityEngine;
using Mirror;

public class HoldItem : NetworkBehaviour
{
    public Vector3 targetLocation;
    public Transform holder;
    public Transform player;
    private float speed = 25.0f; // Speed at which the item will move towards the target location
    public float throwForce = 10.0f; // Amount of force to throw the object
    private float threshold = 3;
    public bool isGrabbing = false;
    private bool isMoving = false;
    private Vector3 startPosition;
    [SyncVar]
    public Vector3 velocity;
    private float startTime;
    private float journeyLength;
    [SyncVar]
    public float Strength = 5f;
    private Rigidbody PlayerRb;
    public Rigidbody rb;

    private float lastToggleTime = 0f;
    private const float toggleCooldown = 0.5f; // Cooldown time in seconds
    public MeshRenderer mrenderer;
    // List to store original materials
    public Material[] originalMaterials;
    // Material with near-invisible alpha
    public Material invisibleMaterial;

    public void Start()
    {
        Setup();
        mrenderer = GetComponent<MeshRenderer>();
       
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Setup();
        
    }

    public void Setup()
    {
        foreach(var con in NetworkServer.connections)
        {
            if (!con.Value.isReady)
            {
                return;
            }
        }
        rb = GetComponent<Rigidbody>();
        holder = GameObject.Find("Holder").transform;
        player = holder.transform.parent;

        // Store the original materials
        originalMaterials = mrenderer.materials;

        // Create the near-invisible material
        invisibleMaterial = new Material(originalMaterials[0]);
        Color color = invisibleMaterial.color;
        color.a = 0.000001f;
        invisibleMaterial.color = color;
    }

    void Update()
    {
        if (isGrabbing)
        {
            //ApplyInvisibleMaterial();
            rb.isKinematic = true;
            targetLocation = holder.position;

            transform.position = Vector3.Lerp(transform.position, targetLocation, speed * Time.deltaTime / (rb.mass / 3));

            if (Vector3.Distance(transform.position, targetLocation) > threshold)
            {
                NetworkUtils.RpcHandler(this, GrabDrop);
            }
        }
        else
        {
           // ApplyOriginalMaterials();
            rb.isKinematic = false;
        }
        velocity = rb.linearVelocity.normalized;
    }

    [ClientRpc]
    private void AddForce_ClientRpc(Vector3 direction, float strength)
    {
        if (PlayerRb != null)
        {
            PlayerRb.AddForce(direction * strength, ForceMode.Impulse);
        }
    }

    public void GrabDrop()
    {
        if (Time.time - lastToggleTime >= toggleCooldown)
        {
            lastToggleTime = Time.time;
            startPosition = transform.position;
            startTime = Time.time;
            journeyLength = Vector3.Distance(startPosition, targetLocation);
            isMoving = true;
            isGrabbing = !isGrabbing;
        }
    }

    public void ThrowLogic()
    {
        isGrabbing = false;
        Vector3 direction = player.forward.normalized;
        rb.isKinematic = false;
        rb.AddForce(direction * throwForce * rb.mass, ForceMode.Impulse);

        Throw_ClientRpc(direction, throwForce);
    }

    [ClientRpc]
    private void Throw_ClientRpc(Vector3 direction, float throwForce)
    {
        rb.isKinematic = false;
        rb.AddForce(direction * throwForce, ForceMode.Impulse);
    }

    public void Throw()
    {
        NetworkUtils.RpcHandler(this, ThrowLogic);
    }

    public void Trigger()
    {
        NetworkUtils.RpcHandler(this, GrabDrop);
    }

    private void ApplyInvisibleMaterial()
    {
        // Apply the invisible material to all renderers
        Material[] materials = new Material[originalMaterials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = invisibleMaterial;
        }
        GetComponent<Renderer>().sharedMaterials = materials;
    }

    private void ApplyOriginalMaterials()
    {
        // Restore the original materials
        GetComponent<Renderer>().sharedMaterials = originalMaterials;
    }
}
