using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float displacementForce = 10f;

    private Vector3 initialForce;

    public void Initialize(Vector3 force)
    {
        initialForce = force;
        if (IsServer)
        {
            ApplyForce();
        }
    }

    void ApplyForce()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(initialForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsPlayerObject)
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = collision.contacts[0].normal * -1f;
                rb.AddForce(forceDirection * displacementForce, ForceMode.Impulse);
            }
        }

        DestroyProjectileServerRpc();
    }

    [ServerRpc]
    void DestroyProjectileServerRpc()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn();
        }
        Destroy(gameObject);
    }
}
