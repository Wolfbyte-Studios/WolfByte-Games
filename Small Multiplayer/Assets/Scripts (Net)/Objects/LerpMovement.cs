using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LerpMovement : NetworkBehaviour
{
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public List<TransformData> transforms = new List<TransformData>();
    private TransformData initialTransform;
    private bool isLerping = false;
    public float lerpSpeed = 1.0f;
    private int currentTargetIndex = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
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
            LerpToTarget();
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

    private void LerpToTarget()
    {
        TransformData targetTransform = currentTargetIndex < transforms.Count ? transforms[currentTargetIndex] : initialTransform;
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed);

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
}
