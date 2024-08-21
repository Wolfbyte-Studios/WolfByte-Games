using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;
using System;


public struct PlayerData 
{
    public int netId;
    public string name;
}

public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }
    public static float CurrentNumberOfPlayers;
    [SyncVar]
    public bool netActive = false;
    [SyncVar]
    public int IndexOfSab = 0;
    public NetworkManager nm;
    [SyncVar]
    public GameObject playertemp;
    public PlayerData lastPlayer;

    public List<NetworkConnectionToClient> clients = new List<NetworkConnectionToClient>();

    public enum GameStateEnum
    {
        InGame,
        UI,
        Other
    }

    public enum GameModeEnum
    {
        Standard
    }
    [SyncVar(hook = nameof(syncModeChanges))]
    public GameModeEnum GameMode = GameModeEnum.Standard;
    [SyncVar(hook = nameof(syncStateChanges))]
    public GameStateEnum GameState = GameStateEnum.UI;
    public readonly SyncList<PlayerData> playersList = new SyncList<PlayerData>();
    public readonly SyncList<string> playernames = new SyncList<string>();

    public void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }
    [ClientRpc]
    public void syncStateChanges(GameStateEnum oldValue, GameStateEnum newValue)
    {
        GameState = newValue;
       
    }
    [ClientRpc]
    public void syncModeChanges(GameModeEnum oldValue, GameModeEnum newValue)
    {
        GameMode = newValue;

    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        nm = NetworkManager.singleton;
        //Instance = this;
        GetPlayers();
    }

    public void Update()
    {
        if (isServer)
        {
            netActive = NetworkServer.active;
            playernames.Clear();
            GetPlayers();
            foreach (var player in playersList)
            {
                playernames.Add(player.name);
            }
        }
    }

    [ContextMenu("Rotate Players")]
    [ClientRpc]
    public void RotatePlayers()
    {
        if (!isServer) return;
        var oldValue = IndexOfSab;
        IndexOfSab = UnityEngine.Random.Range(0, playersList.Count);
        if (playersList.Count > 1)
        {
            if (oldValue == IndexOfSab)
            {
                Start:
                IndexOfSab = UnityEngine.Random.Range(0, playersList.Count);
                if (oldValue == IndexOfSab)
                {
                    goto Start;
                }
            }
        }
        foreach (var conn in NetworkServer.connections)
        {
            var id = conn.Key;
            GameObject playerObj = null;
            if (NetworkServer.connections.TryGetValue(id, out NetworkConnectionToClient connection))
            {
                playerObj = connection.identity.gameObject;
            }
            else
            {
                playerObj = null;
            }

            var refreshScript = playerObj?.GetComponentsInChildren<PlayerNetworkIndex>(true);
            foreach (var script in refreshScript)
            {
                script.gameObject.SetActive(true);
                StartCoroutine(script.Delay());
            }
        }
    }

    
 
    public void GetPlayers()
    {
        clients.Clear();
        foreach (var conn in NetworkServer.connections)
        {
            clients.Add(conn.Value);
            
        }

        playersList.Clear();

        foreach (var conn in clients)
        {
            var playerObj = conn.identity;
            var NetId = conn.connectionId;
            var nameTag = playerObj?.GetComponentInChildren<NameTag>(false);

            if (nameTag != null)
            {
                string name = nameTag.pName.ToString();
                PlayerData p = new PlayerData
                {
                    netId = NetId,
                    name = name
                };

                if (!playersList.Contains(p))
                {
                    playersList.Add(p);
                    ////Debug.Log($"Added player {name} with netId {playerObj.netId} to playersList.");
                }
            }
            else
            {
               // //Debug.LogWarning($"PlayerObject for netId {playerObj.netId} does not have a NameTag component.");
            }
        }

        //Debug.Log($"Total players in list: {playersList.Count}");
    }
}
