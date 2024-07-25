using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct PlayerData : IEquatable<PlayerData>
{
    public uint netId;
    public string name;

    public bool Equals(PlayerData other)
    {
        return netId == other.netId && name.Equals(other.name);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (netId, name).GetHashCode();
    }
}

public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }
    public static float CurrentNumberOfPlayers;
    public bool netActive = false;
    [SyncVar]
    public int IndexOfSab = 0;
    public NetworkManager nm;
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
    [SyncVar]
    public GameModeEnum GameMode = GameModeEnum.Standard;
    [SyncVar]
    public GameStateEnum GameState = GameStateEnum.UI;
    [SyncVar]
    public List<PlayerData> playersList = new List<PlayerData>();
    [SyncVar]
    public SyncList<string> playernames = new SyncList<string>();

    public void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        nm = NetworkManager.singleton;
        Instance = this;
    }

    public void Update()
    {
        netActive = NetworkServer.active;
        playernames.Clear();
        foreach (var player in playersList)
        {
            playernames.Add(player.name);
        }
    }

    [ContextMenu("Rotate Players")]
    public void RotatePlayers()
    {
        if (!isServer) return;
        var oldValue = IndexOfSab[0];
        IndexOfSab[0] = UnityEngine.Random.Range(0, playersList.Count);
        if (playersList.Count > 1)
        {
            if (oldValue == IndexOfSab[0])
            {
                Start:
                IndexOfSab[0] = UnityEngine.Random.Range(0, playersList.Count);
                if (oldValue == IndexOfSab[0])
                {
                    goto Start;
                }
            }
        }
        foreach (var conn in NetworkServer.connections)
        {
            var playerObj = conn .identity;
            var refreshScript = playerObj.GetComponentsInChildren<PlayerNetworkIndex>(true);
            foreach (var script in refreshScript)
            {
                script.gameObject.SetActive(true);
                StartCoroutine(script.delay());
            }
        }
    }

    public void GetPlayers()
    {
        if (isServer)
        {
            clients.Clear();
            foreach (var conn in NetworkServer.connections)
            {
                clients.Add(conn );
            }

            playersList.Clear();

            foreach (var conn in clients)
            {
                var playerObj = conn.identity;
                var nameTag = playerObj.GetComponentInChildren<NameTag>(false);

                if (nameTag != null)
                {
                    string name = nameTag.pName.ToString();
                    PlayerData p = new PlayerData
                    {
                        netId = playerObj.netId,
                        name = name
                    };

                    if (!playersList.Contains(p))
                    {
                        playersList.Add(p);
                    }
                }
                else
                {
                    Debug.LogWarning($"PlayerObject for netId {playerObj.netId} does not have a NameTag component.");
                }
            }
        }
    }
}
