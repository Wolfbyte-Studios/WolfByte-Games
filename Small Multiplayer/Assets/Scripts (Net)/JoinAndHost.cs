using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class JoinAndHost : MonoBehaviour
{
    // Public variable to store the player's network index

    private void Start()
    {
        // Set the network transport address and port
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("127.0.0.1", 9110);
    }

    // Method to start the game as host
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    // Method to start the game as client
    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }

    // Method to be called when the network object spawns
    
}
