using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;

public class JoinAndHost : MonoBehaviour
{
    UnityTransport transport;
    TMP_InputField ip;
    TMP_InputField port;
    public CurrentSessionStats stats;
    public Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    private void Start()
    {
        // Set the network transport address and port
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        ip = transform.FindDeepChild("IP").GetComponent<TMP_InputField>();
        port = transform.FindDeepChild("Port").GetComponent<TMP_InputField>();
        ip.text = PlayerPrefs.GetString("IP", ip.text);
        port.text = PlayerPrefs.GetString("Port", port.text);
        transport.SetConnectionData(ip.text, ushort.Parse(port.text));
        stats = GetComponent<CurrentSessionStats>();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
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

    public void SetConnectionInfo()
    {
        PlayerPrefs.SetString("IP", ip.text);
        PlayerPrefs.SetString("Port", port.text);
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"OnClientConnected: clientId = {clientId}");
        if (NetworkManager.Singleton.IsServer)
        {
            // Server handling
            if (!playerNames.ContainsKey(clientId))
            {
                playerNames[clientId] = PlayerPrefs.GetString("Name");
                Debug.Log($"Server: Added player name for clientId = {clientId}, name = {playerNames[clientId]}");
            }
        }
        else if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Client handling
            string playerName = PlayerPrefs.GetString("Name");
            Debug.Log($"Client: Setting player name for clientId = {clientId}, name = {playerName}");
            applyPlayerNameServerRpc(clientId, playerName);
        }
    }

    [ClientRpc]
    private void updatePlayerNameClientRpc(ulong clientId, string name)
    {
        Debug.Log($"updatePlayerNameClientRpc: clientId = {clientId}, name = {name}");
        if (playerNames.ContainsKey(clientId))
        {
            playerNames[clientId] = name;
        }
        else
        {
            playerNames.Add(clientId, name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void applyPlayerNameServerRpc(ulong clientId, string name)
    {
        Debug.Log($"applyPlayerNameServerRpc: clientId = {clientId}, name = {name}");
        if (!playerNames.ContainsKey(clientId))
        {
            playerNames.Add(clientId, name);
        }
        else
        {
            playerNames[clientId] = name;
        }
        updatePlayerNameClientRpc(clientId, name);
    }

    public void applyPlayerName(string name)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"applyPlayerName: clientId = {clientId}, name = {name}");
        if (NetworkManager.Singleton.IsServer)
        {
            applyPlayerNameServerRpc(clientId, name);
        }
        else
        {
            applyPlayerNameServerRpc(clientId, name);
        }
    }
}
