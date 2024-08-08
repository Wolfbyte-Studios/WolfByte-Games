using UnityEngine;
using Mirror;

public class HoldItem : NetworkBehaviour
{
    public Vector3 targetLocation;
    private Transform player;
    private Transform toucher;
    public float speed = 1.0f; // Speed at which the item will move towards the target location
    public float throwForce = 10.0f; // Amount of force to throw the object
    public float threshold;
    public bool isGrabbing = false;
    private bool isMoving = false;
    private Vector3 startPosition;
    private float startTime;
    private float journeyLength;

    private Rigidbody rb;

    private float lastToggleTime = 0f;
    private const float toggleCooldown = 0.5f; // Cooldown time in seconds

    public override void OnStartClient()
    {
        base.OnStartClient();
        toucher = GameObject.Find("Toucher").transform;
        player = toucher.parent.parent;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isGrabbing)
        {
            rb.isKinematic = true;
            targetLocation = (player.position + toucher.position) / 2;

            transform.position = Vector3.Lerp(transform.position, targetLocation, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetLocation) < threshold)
            {
                isMoving = false;
            }
        }
        else
        {
            rb.isKinematic = false;
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
        Vector3 direction = (toucher.position - transform.position).normalized;
        rb.isKinematic = false;
        rb.AddForce(direction * throwForce, ForceMode.Impulse);

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
