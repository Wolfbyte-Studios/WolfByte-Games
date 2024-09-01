using HeathenEngineering.SteamworksIntegration.UI;
using UnityEngine;
using Steamworks;
using System.Collections;
using JetBrains.Annotations;
using System.Runtime.InteropServices.WindowsRuntime;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;

public class JoinFriend : MonoBehaviour
{
    public FriendProfile friendProfile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        friendProfile = this.gameObject.GetComponent<FriendProfile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void joinGame()
    {
        string lobbyId = friendProfile.UserData.id.ToString();
        SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(lobbyId)));
    }

}
