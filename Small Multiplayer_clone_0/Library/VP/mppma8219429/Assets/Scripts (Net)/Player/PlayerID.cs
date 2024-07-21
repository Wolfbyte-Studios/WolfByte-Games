using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Collections;

public class PlayerNetworkIndex : NetworkBehaviour
{
    // Public variable to store the player's network index
    public int playerIndexTarget;
    public GameObject playerCamera;
    public enum playerType
    {
        Sab,
        Runner
    }
    public playerType PlayerType;

    public NetworkVariable<bool> isEnabled = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void OnEnable()
    {
        if (CurrentSessionStats.Instance.netActive)
        {
            ApplyState();
        }
    }

    public void Start()
    {
        // Ensure the local state matches the network state
        ApplyState();
    }

    // OnNetworkSpawn is called when the object is initialized on the network
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize state on the server
            isEnabled.Value = gameObject.activeSelf;
        }

        // Subscribe to state changes
        isEnabled.OnValueChanged += OnIsEnabledChanged;

        // Only assign the playerIndex on the local player
        if (!IsOwner)
        {
            // Disable the camera if this is not the local player
            playerCamera.gameObject.SetActive(false);
        }
        else
        {
            // Ensure the local player's camera is enabled
            playerCamera.gameObject.SetActive(true);
        }

        
            StartCoroutine(delay());
        
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from state changes
        isEnabled.OnValueChanged -= OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        ApplyState();
    }

    private void ApplyState()
    {
        gameObject.SetActive(isEnabled.Value);
    }

    
    public void SetEnabledState(bool state)
    {
        isEnabled.Value = state;
        NetworkUtils.RpcHandler(this, ApplyState);
    }

    [ContextMenu("Enable Object")]
    public void EnableObject()
    {
        if (IsServer)
        {
            SetEnabledState(true);
        }
        else
        {
            SetEnabledState(true);
        }
    }

    [ContextMenu("Disable Object")]
    public void DisableObject()
    {
        if (IsServer)
        {
            SetEnabledState(false);
        }
        else
        {
            SetEnabledState(false);
        }
    }

    public IEnumerator delay()
    {
        if (IsServer)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yield return new WaitForSeconds(2);

            refresh();
        }

        yield return null;
    }

    public void refresh()
    {
        int localId = (int)this.NetworkObject.OwnerClientId;
        int sabId = CurrentSessionStats.Instance.IndexOfSab.Value;

        switch (PlayerType)
        {
            case playerType.Sab:
                if (sabId == localId)
                {
                    EnableObject();
                }
                else
                {
                    DisableObject();
                }
                break;
            case playerType.Runner:
                if (sabId != localId)
                {
                    EnableObject();
                }
                else
                {
                    DisableObject();
                }
                break;
        }
    }
}
