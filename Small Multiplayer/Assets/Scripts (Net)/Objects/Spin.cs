using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkRigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class Spin : NetworkBehaviour
{
    public float spinSpeed;
    public float oldSpeed;
    public Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = gameObject.GetComponent<Rigidbody>();
        oldSpeed = spinSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed);
        rb.rotation = transform.rotation;
    }
    public void overrideSpeed(float speed)
    {
        spinSpeed = spinSpeed * speed;
    }
    public void Reset()
    {
        spinSpeed = oldSpeed;
    }
}
