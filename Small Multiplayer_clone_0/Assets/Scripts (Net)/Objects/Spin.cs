using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkRigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class Spin : NetworkBehaviour
{
    public float spinSpeed;
    private float oldSpeed;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        oldSpeed = spinSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            rb.rotation = transform.rotation;
            //Debug.Log($"Spinning with speed: {spinSpeed}");
        }
    }

    [ServerRpc]
    public void OverrideSpeedServerRpc(float speed)
    {
        spinSpeed *= speed;
        OverrideSpeedClientRpc(spinSpeed);
    }

    [ClientRpc]
    private void OverrideSpeedClientRpc(float newSpeed)
    {
        spinSpeed = newSpeed;
        //Debug.Log($"Speed overridden to: {spinSpeed}");
    }

    public void OverrideSpeed(float speed)
    {
        if (IsOwner)
        {
            OverrideSpeedServerRpc(speed);
        }
    }

    [ServerRpc]
    public void ResetSpeedServerRpc()
    {
        spinSpeed = oldSpeed;
        ResetSpeedClientRpc(spinSpeed);
    }

    [ClientRpc]
    private void ResetSpeedClientRpc(float newSpeed)
    {
        spinSpeed = newSpeed;
        //Debug.Log($"Speed reset to: {spinSpeed}");
    }

    public void Reset()
    {
        if (IsOwner)
        {
            ResetSpeedServerRpc();
        }
    }
}
