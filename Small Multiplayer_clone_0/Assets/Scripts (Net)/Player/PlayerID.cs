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

    [SyncVar (hook = nameof(OnIsEnabledChanged))]
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

        
        // Subscribe to state changes
        

        // Only assign the playerIndex on the local player
        if (!isLocalPlayer)
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

    public override void OnStopClient()
    {
        // Unsubscribe from state changes
       // isEnabled. -= OnIsEnabledChanged;
    }

    private void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        delay();
    }

    private void ApplyState()
    {
        gameObject.SetActive(isEnabled );
    }

    
    public void SetEnabledState(bool state)
    {
        isEnabled  = state;
        ApplyState();
    }

   
    
    
    public IEnumerator delay()
    {
        if (isServer)
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
        localId = NetworkClient.connection.connectionId;
        sabId = CurrentSessionStats.Instance.IndexOfSab ;

        switch (PlayerType)
        {
            case playerType.Sab:
                if (sabId == localId)
                {
                    SetEnabledState(true);
                }
                else
                {
                    SetEnabledState(false);
                }
                break;
            case playerType.Runner:
                if (sabId != localId)
                {
                    SetEnabledState(true);
                }
                else
                {
                    SetEnabledState(false);
                }
                break;
        }
    }
}
