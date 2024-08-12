using System.Collections.Generic;
using UnityEngine;
using Mirror;
 

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
public class Spin : NetworkBehaviour
{
    public float spinSpeed;
    private float oldSpeed;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        oldSpeed = spinSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOwned)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            rb.rotation = transform.rotation;
            //Debug.Log($"Spinning with speed: {spinSpeed}");
        }
    }

    [Command]
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
        if (isOwned)
        {
            OverrideSpeedServerRpc(speed);
        }
    }

    [Command]
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
        if (isOwned)
        {
            ResetSpeedServerRpc();
        }
    }
}
