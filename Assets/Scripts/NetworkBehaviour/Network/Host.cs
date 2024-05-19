using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;
public class Connections : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public string Ip;
    public float Port;
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        Ip = LoadTheLoadingScreen.IP; 
        Port = LoadTheLoadingScreen.Port;
        switch (LoadTheLoadingScreen.playMode)
        {
            case LoadTheLoadingScreen.PlayModes.Single:
                //singleplayer logic here
                break;
            case LoadTheLoadingScreen.PlayModes.Host:
                host();
                break;
            case LoadTheLoadingScreen.PlayModes.Client:
                ConnectToServer();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void host()
    {
        NetworkManager.StartHost();
        Destroy(gameObject);
    }
    public void ConnectToServer()
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
        unityTransport.ConnectionData.Address = Ip;
        unityTransport.ConnectionData.Port = (ushort)Port;

        // Start client to connect to the server
        NetworkManager.StartClient();
        gameObject.SetActive(false);    }
   
}