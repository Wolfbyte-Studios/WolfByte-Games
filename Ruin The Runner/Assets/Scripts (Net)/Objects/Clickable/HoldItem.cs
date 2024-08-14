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

    public void Start()
    {
        Setup();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        Setup();
    }

    public void Setup()
    {
        rb = GetComponent<Rigidbody>();
        holder = GameObject.Find("Holder").transform;
        player = holder.transform.parent;
    }

    void Update()
    {
        var renderer = GetComponent<MeshRenderer>();
        var origColor = renderer.material.color;
        if (isGrabbing)
        {
            rb.isKinematic = true;
            targetLocation = holder.position;

            transform.position = Vector3.Lerp(transform.position, targetLocation, speed * Time.deltaTime / (rb.mass / 3));

            if (Vector3.Distance(transform.position, targetLocation) > threshold)
            {
                NetworkUtils.RpcHandler(this, GrabDrop);
            }
            var newcolor = origColor;
            newcolor.a = 0.0000001f;
            renderer.material.color = newcolor;
        }
        else
        {
            rb.isKinematic = false;
           
            gameObject.GetComponent<MeshRenderer>().material.color = origColor;
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
}
