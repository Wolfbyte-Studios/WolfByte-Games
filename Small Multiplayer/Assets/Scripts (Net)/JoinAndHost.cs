using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.UIElements;

public class JoinAndHost : MonoBehaviour
{
    // Public variable to store the player's network index
    UnityTransport transport;
    TMP_InputField ip;
    TMP_InputField port;

    private void Start()
    {
        // Set the network transport address and port
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        ip = transform.FindDeepChild("IP").GetComponent<TMP_InputField>();
        port = transform.FindDeepChild("Port").GetComponent<TMP_InputField>();
        ip.text = PlayerPrefs.GetString("IP", ip.text);
        port.text = PlayerPrefs.GetString("Port", port.text);
        transport.SetConnectionData(ip.text, ushort.Parse( port.text));
    }

    // Method to start the game as host
    public void StartHost()
    {
        
        transport.SetConnectionData("0.0.0.0", ushort.Parse(transform.FindDeepChild("Port").GetComponent<TMP_InputField>().text));
        Debug.Log(ip.text + " " + port.text);
        NetworkManager.Singleton.StartHost();
    }

    // Method to start the game as client
    public void JoinGame()
    {
        
        transport.SetConnectionData(transform.FindDeepChild("IP").GetComponent<TMP_InputField>().text, ushort.Parse(transform.FindDeepChild("Port").GetComponent<TMP_InputField>().text));
        NetworkManager.Singleton.StartClient();
    }

    // Method to be called when the network object spawns
    
}
