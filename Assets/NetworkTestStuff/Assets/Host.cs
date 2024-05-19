using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Host : MonoBehaviour
{
    public NetworkManager NetworkManager;
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager = GameObject.Find("Network").GetComponent<NetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void host()
    {
        NetworkManager.StartHost();
    }
    public void connect(string ip)
    {
        ConnectToServer(ip, 7777);
    }
    public void ConnectToServer(string ipAddress, int port)
    {
        // Check if the NetworkManager is available
        if (NetworkManager == null)
        {
            Debug.LogError("NetworkManager component not found!");
            return;
        }

        // Set up the network transport with the given IP and port
        var unityTransport = NetworkManager.GetComponent<UnityTransport>();
        if (unityTransport == null)
        {
            Debug.LogError("UnityTransport component not found!");
            return;
        }

        // Configure the connection data
        unityTransport.ConnectionData.Address = ipAddress;
        unityTransport.ConnectionData.Port = (ushort)port;

        // Start client to connect to the server
        NetworkManager.StartClient();
    }
}
