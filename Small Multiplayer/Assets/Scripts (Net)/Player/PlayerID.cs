using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Collections;

public class PlayerNetworkIndex : NetworkBehaviour
{
    // Public variable to store the player's network index
    public int playerIndexTarget;
    public GameObject playerCamera;
    [SyncVar]
    public int sabId;
    [SyncVar]
    public int localId;

    public enum playerType
    {
        Sab,
        Runner
    }
    public playerType PlayerType;

    [SyncVar(hook = nameof(OnIsEnabledChanged))]
    public bool isEnabled = true;

    public void OnEnable()
    {
        if (CurrentSessionStats.Instance == null) { return; }
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

    // OnStartClient is called when the object is initialized on the network
    public override void OnStartClient()
    {
        // Initialize state on the server
        isEnabled = gameObject.activeSelf;

        // Only assign the playerIndex on the local player
        if (!isLocalPlayer)
        {
            // Disable the camera if this is not the local player
            playerCamera.SetActive(false);
        }
        else
        {
            // Ensure the local player's camera is enabled
            playerCamera.SetActive(true);
        }

        StartCoroutine(Delay());
    }

    public override void OnStopClient()
    {
        // Unsubscribe from state changes
    }

    private void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        ApplyState();
    }

    private void ApplyState()
    {
        gameObject.SetActive(isEnabled);
    }

    public void SetEnabledState(bool state)
    {
        isEnabled = state;
        ApplyState();
    }

   
    private void RpcRefresh()
    {
        sabId = CurrentSessionStats.Instance.IndexOfSab;
        if (isLocalPlayer)
        {
           
            localId = connectionToClient.connectionId;
            //Debug.Log(connectionToClient.connectionId + " Is my ID");
        }
        switch (PlayerType)
        {
            case playerType.Sab:
                SetEnabledState(sabId == localId);
                break;
            case playerType.Runner:
                SetEnabledState(sabId != localId);
                break;
        }
    }

    [Server]
    public IEnumerator Delay()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(2);

        RpcRefresh();
    }
}
