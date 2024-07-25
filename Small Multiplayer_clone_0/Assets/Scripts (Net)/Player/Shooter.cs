using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Shooter : NetworkBehaviour
{
    [Header("Firing Settings")]
    public float CoolDown = 1.0f;
    public List<GameObject> ammo = new List<GameObject>();
    public float fireForce = 1000f;
    public Transform firePoint; // The point from which bullets are fired

    private float lastFired;

    public GameObject playerCam;
    public InputActionAsset actions;
    public InputAction Fire;
    public Vector3 targetPoint;

    void Start()
    {
        var playerInput = gameObject.GetComponent<PlayerInput>();
        playerCam = transform.parent.FindDeepChildByTag("MainCamera").gameObject;
        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            Fire = actions.FindAction("Fire");
            Fire.performed += OnFire;
        }
    }

    private void OnFire(InputAction.CallbackContext obj)
    {
        if (!IsOwner)
        {
            return;
        }
        if (Time.time - lastFired >= CoolDown)
        {
            lastFired = Time.time;
            RequestFireServerRpc(firePoint.position, targetPoint);
        }
    }

    [ServerRpc]
    private void RequestFireServerRpc(Vector3 firePosition, Vector3 targetPosition)
    {
        FireBullet(firePosition, targetPosition);
    }

    private void FireBullet(Vector3 firePosition, Vector3 targetPosition)
    {
        var bulletPrefab = ammo[Random.Range(0, ammo.Count)];
        var bullet = Instantiate(bulletPrefab, firePosition, Quaternion.identity);

        // Ensure the bullet has a NetworkObject component
        var networkObject = bullet.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            networkObject = bullet.AddComponent<NetworkObject>();
        }

        // Spawn the bullet on the network
        networkObject.Spawn();

        // Add Rigidbody and NetworkRigidbody components if they don't exist
        var rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        Vector3 direction = (targetPosition - firePosition).normalized;
        rb.AddForce(direction * fireForce, ForceMode.Impulse);

        bullet.GetComponent<Projectile>().Initialize(direction * fireForce);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100); // Set a far away point if nothing is hit
        }

        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2.0f);
    }
}
