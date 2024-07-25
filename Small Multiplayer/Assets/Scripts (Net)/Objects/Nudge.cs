using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Netcode.Components;
using System.Collections;
public class Nudge : NetworkBehaviour
{
    public float shakeDuration = 1.0f; // Duration of the shake in seconds
    public float shakeMagnitude = 0.1f; // Magnitude of the shake
    public Dizzy player;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient && !IsServer)
        {
            RequestOriginalPositionServerRpc();
        }
        originalPosition = transform.position;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOriginalPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        ApplyOriginalPositionClientRpc(transform.position, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ApplyOriginalPositionClientRpc(Vector3 position, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            originalPosition = position;
        }
    }
   
    public void StartShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(Shake());
    }
    IEnumerator Shake()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            float z = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = originalPosition + new Vector3(x, y, z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = originalPosition;
    }

    public void TriggerShake()
    {
        if (IsServer)
        {
            StartShake();
            
            TriggerShakeClientRpc();
        }
        else
        {
            TriggerShakeServerRpc();
        }
    }
    public void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.gameObject.tag == "Player" && collision.gameObject.layer == 31)
        {
            player = collision.gameObject.GetComponent<Dizzy>();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerShakeServerRpc()
    {
        StartShake();
        player.TriggerDizzy();
        player = null;
        TriggerShakeClientRpc();
    }

    [ClientRpc]
    private void TriggerShakeClientRpc()
    {
        if (!IsServer)
        {
            player.TriggerDizzy();
            player = null;
            StartShake();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originalPosition, 0.1f);
        }
    }
}
