using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public Rigidbody rb;
    public InputActionAsset actions;
    public float movespeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }


        
    }
    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        rb.AddForce(new Vector3(v.x, 0 , v.y) * movespeed);
    }
}
