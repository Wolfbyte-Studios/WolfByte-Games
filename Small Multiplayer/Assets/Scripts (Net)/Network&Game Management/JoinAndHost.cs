using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using Steamworks;

public class JoinAndHost : MonoBehaviour
{
    UnityTransport transport;
    TMP_InputField ip;
    TMP_InputField port;
    TMP_InputField steamUsername;

    private void Start()
    {
        // Set the network transport address and port
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        ip = transform.FindDeepChild("IP").GetComponent<TMP_InputField>();
        port = transform.FindDeepChild("Port").GetComponent<TMP_InputField>();
        steamUsername = transform.FindDeepChild("SteamUsername").GetComponent<TMP_InputField>();

        ip.text = PlayerPrefs.GetString("IP", ip.text);
        port.text = PlayerPrefs.GetString("Port", port.text);

        transport.SetConnectionData(ip.text, ushort.Parse(port.text));
        //NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        NetworkManager.Singleton.DisconnectClient(obj);
        throw new System.NotImplementedException();
    }

    // Method to start the game as host
    public void StartHost()
    {
        transport.SetConnectionData("0.0.0.0", ushort.Parse(port.text));
        Debug.Log(ip.text + " " + port.text);
        NetworkManager.Singleton.StartHost();
    }

    // Method to start the game as client
    public void JoinGame()
    {
        transport.SetConnectionData(ip.text, ushort.Parse(port.text));
        NetworkManager.Singleton.StartClient();
    }

    public void SetConnectionInfo()
    {
        PlayerPrefs.SetString("IP", ip.text);
        PlayerPrefs.SetString("Port", port.text);
    }

    public void JoinViaSteam()
    {
        string friendName = steamUsername.text;
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized.");
            return;
        }

        CSteamID friendSteamID = GetFriendSteamID(friendName);
        if (friendSteamID == CSteamID.Nil)
        {
            Debug.LogError("Friend not found on Steam.");
            return;
        }

        // Fetch the IP address using Steam P2P networking
        SteamNetworkingIPAddr ipAddr = GetSteamIPAddress(friendSteamID);
        if (ipAddr.IsIPv4())
        {
            string friendIP = ipAddr.ToString();
            Debug.Log("Friend's IP: " + friendIP);

            transport.SetConnectionData(friendIP, ushort.Parse(port.text));
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogError("Failed to get IP address from Steam.");
        }
    }

    private CSteamID GetFriendSteamID(string friendName)
    {
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            if (SteamFriends.GetFriendPersonaName(friendSteamID) == friendName)
            {
                return friendSteamID;
            }
        }
        return CSteamID.Nil;
    }

    private SteamNetworkingIPAddr GetSteamIPAddress(CSteamID friendSteamID)
    {
        SteamNetworkingIPAddr ipAddr = new SteamNetworkingIPAddr();
        // Assume we have a method to get IP from Steam (depends on your Steam P2P implementation)
        // Fill in the logic here to retrieve the IP address from Steam P2P
        // This is a placeholder implementation
        return ipAddr;
    }
}
