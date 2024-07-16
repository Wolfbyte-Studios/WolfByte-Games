using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;

using Unity.Collections;
using NUnit.Framework;
using Unity.VisualScripting;



[Serializable]
public struct PlayerDataInspector
{
    public ulong clientId;
    public string name;

   
}

public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }

    public static float CurrentNumberOfPlayers;
    public float IndexOfSab;
    public float IndexOfRunner1;
    public float IndexOfRunner2;
    public float IndexOfRunner3;
    public UnityTransport transport;
    public NetworkManager nm;

    

    [SerializeField]
    public List<PlayerDataInspector> playersList = new List<PlayerDataInspector>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Start();

    }

    void Start()
    {
        nm = NetworkManager.Singleton;
        transport = nm.GetComponent<UnityTransport>();
        playersList = new List<PlayerDataInspector>();

        
    }
    public void Update()
    {
        UpdateServerRpc();
    }
    [ServerRpc]
    public void UpdateServerRpc()
    {
        GetPlayers();
        UpdateClientRpc();
    }
    [ClientRpc]
    public void UpdateClientRpc()
    {
        GetPlayers();
    }
    public void GetPlayers()
    {
        foreach(var player in nm.ConnectedClientsIds)
        {
            var playerObj = nm.ConnectedClientsList[0].PlayerObject;
            var name = playerObj.GetComponentInChildren<NameTag>(false).name;
            PlayerDataInspector p = new PlayerDataInspector();
            p.clientId = playerObj.OwnerClientId;
            p.name = name;
            if (!playersList.Contains(p))
            {
                playersList.Add(p);
            }
        }
        //playersList.Add(;
    }

    
   

}
