using Mirror;
using UnityEngine;

public class MalletForce : NetworkBehaviour
{
    [SyncVar]
    public float force;
    [SyncVar]
    public GameObject target;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        target = other.gameObject;
        //Debug.Log(target.name);
    }
    public void addForceToObject()
    {
        var targetRb = GetComponent<Rigidbody>();
        targetRb.AddForce((rb.linearVelocity * force) / targetRb.mass);
    }
}
