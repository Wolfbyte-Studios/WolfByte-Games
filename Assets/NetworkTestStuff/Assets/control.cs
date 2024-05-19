using Unity.Netcode;
using UnityEngine;

public class Control : NetworkBehaviour
{
    public Rigidbody rb;
    public Camera cam;
    public float moveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure Rigidbody is properly initialized
        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        cam = Camera.main;
    }

    // FixedUpdate is called once per physics update
    void FixedUpdate()
    {
        // Ensure that only the owner of this network object can move it
        if (!IsOwner) return;

        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");

        if (Mathf.Abs(hor) > 0)
        {
            rb.AddForce(new Vector3(hor * moveSpeed, 0, 0));
        }
        if (Mathf.Abs(ver) > 0)
        {
            rb.AddForce(new Vector3(0, 0, ver * moveSpeed));
        }
    }
}
