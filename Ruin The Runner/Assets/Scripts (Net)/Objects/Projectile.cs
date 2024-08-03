using UnityEngine;
using Mirror;
using Mirror.Examples.CCU;

public class Projectile : NetworkBehaviour
{
    public float displacementForce = 10f;
    private Vector3 initialForce;

    public void Initialize(Vector3 force)
    {
        initialForce = force;
        if (isServer)
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
        if (!isServer) return;

        NetworkIdentity networkIdentity = collision.gameObject.GetComponent<NetworkIdentity>();
        if (networkIdentity != null && IsPlayerObject(networkIdentity))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = collision.contacts[0].normal * -1f;
                rb.AddForce(forceDirection * displacementForce, ForceMode.Impulse);
            }
        }

        DestroyProjectile();
    }

    private bool IsPlayerObject(NetworkIdentity networkIdentity)
    {
        // Check if the object has a Player component or tag
        return networkIdentity.GetComponent<Player>() != null || networkIdentity.CompareTag("Player");
    }

    [Server]
    void DestroyProjectile()
    {
        NetworkServer.Destroy(gameObject);
    }
}
