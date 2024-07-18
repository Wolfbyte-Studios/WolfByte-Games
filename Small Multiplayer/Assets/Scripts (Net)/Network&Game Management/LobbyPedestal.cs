using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class LobbyPedestal : NetworkBehaviour
{
    public List<GameObject> target;
    public List<GameObject> players;
    public GameObject pedestal;
    public int test = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        target = new List<GameObject>();
        players = new List<GameObject>();
        if (IsServer)
        {
            NetworkUtils.RpcHandler(this, updateGameModes);
        }
    }

    public void updateGameModes()
    {
        
        CurrentSessionStats.Instance.GameState.Value = CurrentSessionStats.GameStateEnum.UI;
        CurrentSessionStats.Instance.GameMode.Value =  CurrentSessionStats.GameModeEnum.Standard;
    }
    


    // Update is called once per frame
    void Update()
    {
        if (CurrentSessionStats.Instance.netActive)
        {
            NetworkUtils.RpcHandler(this, updatePostitions);
        }
    }
    public void updatePostitions()
    {
        var pedestalParent = GameObject.Find("Player List");


        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {

            if (!players.Contains(g))
            {
                players.Add(g);
            }


        }

        foreach (var player in players)
        {



            //Debug.Log(player.name);
            int id = (int)player.transform.parent.GetComponent<NetworkObject>().OwnerClientId;
            test = id;
            pedestal = pedestalParent.transform.Find("Player " + (id + 1).ToString()).gameObject;

            if (!target.Contains(pedestal))
            {
                target.Insert(id, pedestal);
            }

            player.transform.position = target[id].transform.position;
        }
    }
}
