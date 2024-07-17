using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PlayerNetworkIndex : NetworkBehaviour
{
    // Public variable to store the player's network index
    public int playerIndexTarget;
    public GameObject playerCamera;
    public void Start()
    {
        playerCamera = transform.parent.FindDeepChildByTag("MainCamera").gameObject;
        if(transform.parent.FindDeepChildrenByTag("MainCamera")[1].gameObject != playerCamera)
        {
            if (!IsOwner)
            {
                transform.parent.FindDeepChildrenByTag("MainCamera")[1].gameObject.SetActive(false);
            }
        }
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
    }
    // OnNetworkSpawn is called when the object is initialized on the network
    public override void OnNetworkSpawn()
    {
        
        var netman = gameObject.transform.parent.GetComponent<NetworkObject>();
        // Only assign the playerIndex on the local player
        var index = netman.OwnerClientId.ConvertTo<int>();

        if (index == playerIndexTarget)
        {
            return;
        }
        this.gameObject.SetActive(false);
        

    }
}
