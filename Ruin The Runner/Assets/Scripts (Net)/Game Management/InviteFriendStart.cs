using HeathenEngineering.SteamworksIntegration.UI;
using UnityEngine;
using Steamworks;
using System.Collections;
using JetBrains.Annotations;
using System.Runtime.InteropServices.WindowsRuntime;

public class InviteFriendStart : MonoBehaviour
{
    public string friendId;
    public FriendProfile friendProfile;
    public FriendList friendList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        friendProfile = this.gameObject.GetComponent<FriendProfile>();
        friendList = GetComponent<FriendList>();
        if(friendList != null)
        {
            friendList.UpdateDisplay();
        }
       
        //UpdateList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void inviteFriend()
    {
        friendProfile.UserData.InviteToGame("Start");
    }
    IEnumerator updateList()
    {
        
        yield return new WaitForSeconds(.1f);
        friendList.UpdateDisplay();
        yield return null;
    }
    public void UpdateList()
    {
        StartCoroutine(updateList());
    }
}
