using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class LobbyPedestal : NetworkBehaviour
{
    public List<GameObject> target;
    public List<GameObject> players;
    public GameObject pedestal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        target = new List<GameObject>();
        players = new List<GameObject>();
        if (isServer)
        {
            updateGameModes();
        }
    }

    public void updateGameModes()
    {
        
        CurrentSessionStats.Instance.GameState  = CurrentSessionStats.GameStateEnum.UI;
        CurrentSessionStats.Instance.GameMode  =  CurrentSessionStats.GameModeEnum.Standard;
    }
    


    // Update is called once per frame
    void Update()
    {
        if (CurrentSessionStats.Instance.netActive)
        {
            updatePostitions();
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



            ////Debug.Log(player.name);
            int id = (int)player.transform.parent.GetComponent<NetworkIdentity>().netId;
            pedestal = pedestalParent.transform.Find("Player " + (id + 1).ToString()).gameObject;

            if (!target.Contains(pedestal))
            {
                target.Insert(id, pedestal);
            }

            player.transform.position = target[id].transform.position;
            player.transform.localEulerAngles = new Vector3(0, 180, 0);
        }
    }
}
