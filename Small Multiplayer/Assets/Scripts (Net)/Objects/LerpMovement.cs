using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class LerpMovement : NetworkBehaviour
{
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
    }
    
    public List<TransformData> transforms = new List<TransformData>();
    private TransformData initialTransform;
    private bool isLerping = false;
    public float lerpSpeed = 1.0f;
    public bool Stationary = true;
    private int currentTargetIndex = 0;
    public bool rigBody = true;
    private Rigidbody rb;
    private NetworkRigidbody networkRb;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (rigBody)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            networkRb = gameObject.AddComponent<NetworkRigidbody>();
            rb.isKinematic = Stationary;
        }
        initialTransform.position = transform.position;
        initialTransform.rotation = transform.rotation;
        transform.position = initialTransform.position;
        if (IsClient && !IsServer)
        {
            RequestInitialTransformServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInitialTransformServerRpc(ServerRpcParams rpcParams = default)
    {
        
        // ApplyInitialTransformClientRpc(initialTransform.position, initialTransform.rotation, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ApplyInitialTransformClientRpc(Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            transform.position = position;
            transform.rotation = rotation;
            initialTransform.position = position;
            initialTransform.rotation = rotation;
        }
    }

    void Update()
    {
        if (IsServer && isLerping)
        {
            
            if (rigBody)
            {
                LerpToTarget();
                rb.isKinematic = Stationary;
            }
            else
            {
                LerpToTargetNoRB();
            }
    }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerServerRpc()
    {
        if (transforms.Count > 0)
        {
            isLerping = true;
            currentTargetIndex = 0;
            TriggerClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerClientRpc()
    {
        if (!IsServer)
        {
            isLerping = true;
            currentTargetIndex = 0;
        }
    }

    public void Trigger()
    {
        if (IsServer)
        {
            isLerping = true;
            currentTargetIndex = 0;
            TriggerClientRpc();
        }
        else
        {
            TriggerServerRpc();
        }
    }
    public void LerpToTargetNoRB()
    {
        TransformData targetTransform = currentTargetIndex < transforms.Count ? transforms[currentTargetIndex] : initialTransform;
        if (targetTransform.speed == 0)
        {
            targetTransform.speed = 1;
        }
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed * targetTransform.speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed * targetTransform.speed);

        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 1.0f)
        {
            currentTargetIndex++;
            if (currentTargetIndex > transforms.Count)
            {
                isLerping = false;
                currentTargetIndex = 0;
            }
        }
    }
    public void LerpToTarget()
    {
        TransformData targetTransform = currentTargetIndex < transforms.Count ? transforms[currentTargetIndex] : initialTransform;
        if (targetTransform.speed == 0)
        {
            targetTransform.speed = 1;
        }

        Vector3 newPosition = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed * targetTransform.speed);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed * targetTransform.speed);

        rb.MovePosition(newPosition);
        rb.MoveRotation(newRotation);

        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 1.0f)
        {
            currentTargetIndex++;
            if (currentTargetIndex > transforms.Count)
            {
                isLerping = false;
                currentTargetIndex = 0;
            }
        }
    }

    public float CalculateEstimatedTime()
    {
        float totalDistance = 0f;
        Vector3 previousPosition = initialTransform.position;

        // Calculate the distance between each consecutive transform and the initial position
        foreach (var transformData in transforms)
        {
            totalDistance += Vector3.Distance(previousPosition, transformData.position);
            previousPosition = transformData.position;
        }

        // Add the distance from the last transform back to the initial position
        totalDistance += Vector3.Distance(previousPosition, initialTransform.position);

        // Calculate total time
        float totalTime = totalDistance / lerpSpeed;
        return totalTime;
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            float estimatedTime = CalculateEstimatedTime();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(initialTransform.position, 0.1f);
            ////Debug.Log("Estimated Total Lerp Time: " + estimatedTime + " seconds");
        }
    }
}
