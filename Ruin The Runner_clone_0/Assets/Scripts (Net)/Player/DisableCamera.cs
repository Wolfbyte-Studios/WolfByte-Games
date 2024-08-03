using UnityEngine;
using Mirror;
using Mirror.Examples.BilliardsPredicted;

public class DisableCamera : NetworkBehaviour
{
    // Reference to the camera component
    public Camera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisableIfNotLocal();
    }

    private void OnEnable()
    {
        DisableIfNotLocal();
    }

    [ClientCallback]
    public void DisableIfNotLocal()
    {
        // Ensure playerCamera is assigned
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }
        gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>().enabled = isLocalPlayer;
        if (playerCamera != null)
        {
            // Only enable the camera if this is the local player
            playerCamera.gameObject.SetActive(isLocalPlayer);
        }
    }
}
