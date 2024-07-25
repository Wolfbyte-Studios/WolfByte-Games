using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Netcode.Components;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkRigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class Sink : NetworkBehaviour
{
    public float sinkAmount;
    public float speed;
    public bool isSinking;
    public bool isRising;
    private Rigidbody rb;
    public float originalHeight;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        originalHeight = transform.position.y;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            
            if (isSinking && !isRising)
            {
                var lowPosition = new Vector3(transform.position.x, originalHeight - sinkAmount, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, lowPosition, speed * Time.deltaTime);
            }
            else if (!isSinking && isRising)
            {
                var highPosition = new Vector3(transform.position.x, originalHeight, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, highPosition, speed * Time.deltaTime);
            }
            

            // Check if the object has reached the original height or the low position
            if (Mathf.Abs(transform.position.y - originalHeight) < 0.01f)
            {
                isSinking = false;
                isRising = false;
            }
        }
    }

    [ServerRpc]
    public void SinkServerRpc()
    {
        isSinking = true;
        isRising = false;
        SinkClientRpc();
    }

    [ClientRpc]
    private void SinkClientRpc()
    {
        isSinking = true;
        isRising = false;
    }

    public void sink()
    {
        if (IsOwner)
        {
            SinkServerRpc();
        }
    }

    [ServerRpc]
    public void ResetSinkServerRpc()
    {
        isSinking = false;
        isRising = true;
        ResetSinkClientRpc();
    }

    [ClientRpc]
    private void ResetSinkClientRpc()
    {
        isSinking = false;
        isRising = true;
    }

    public void Reset()
    {
        if (IsOwner)
        {
            ResetSinkServerRpc();
        }
    }
}
