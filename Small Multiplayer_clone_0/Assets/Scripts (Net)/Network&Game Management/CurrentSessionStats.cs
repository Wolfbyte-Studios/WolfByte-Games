using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using AYellowpaper.SerializedCollections;
using Unity.Collections;

[System.Serializable]
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializeByMemcpy
{
    public ulong clientId;
    public FixedString64Bytes name;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && name.Equals(other.name);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (clientId, name).GetHashCode();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
    }
}

[GenerateSerializationForType(typeof(PlayerData))]
public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }
    public static float CurrentNumberOfPlayers;
    public bool netActive = false;
    public NetworkVariable<int> IndexOfSab = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public UnityTransport transport;
    public NetworkManager nm;
    public GameObject playertemp;
    public PlayerData lastPlayer;
    public List<NetworkClient> clients = new List<NetworkClient>();

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

    public NetworkVariable<GameModeEnum> GameMode = new NetworkVariable<GameModeEnum>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<GameStateEnum> GameState = new NetworkVariable<GameStateEnum>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<PlayerData> playersList = new NetworkList<PlayerData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public List<string> playernames = new List<string>();

    public void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        nm = NetworkManager.Singleton;
        Instance = this;
    }

    public void Update()
    {
        netActive = NetworkManager.Singleton.IsListening;
        playernames.Clear();
        foreach (var player in playersList)
        {
            playernames.Add(player.name.ToString());
        }

        NetworkUtils.RpcHandler(this, GetPlayers);
        Debug.Log("I am Server");
    }

    [ContextMenu("Rotate Players")]
    public void RotatePlayers()
    {
        NetworkUtils.RpcHandler(this, doRotation);
    }
    public void doRotation() 
    {
        if (!IsServer) return;
        var oldValue = IndexOfSab.Value;
        IndexOfSab.Value = UnityEngine.Random.Range(0, playersList.Count);
        if (playersList.Count > 1)
        {
            if (oldValue == IndexOfSab.Value)
            {
                Start:
                IndexOfSab.Value = UnityEngine.Random.Range(0, playersList.Count);
                if (oldValue == IndexOfSab.Value)
                {
                    goto Start;
                }
            }
        }
        foreach (var p in NetworkManager.Singleton.ConnectedClientsList)
        {
            var refreshScript = p.PlayerObject.gameObject.GetComponentsInChildren<PlayerNetworkIndex>(true);
            foreach (var script in refreshScript)
            {
                script.gameObject.SetActive(true);
                StartCoroutine(script.delay());
            }

        }
    }

    public void GetPlayers()
    {
        if (IsServer)
        {
            clients.Clear();
            foreach (NetworkClient n in nm.ConnectedClientsList)
            {
                clients.Add(n);
            }

            playersList.Clear();

            foreach (var player in clients)
            {
                var playerObj = player.PlayerObject;
                Debug.Log(playerObj.name);
                var nameTag = playerObj.gameObject.GetComponentInChildren<NameTag>(false);

                if (nameTag != null)
                {
                    string name = nameTag.pName.Value.ToString();
                    PlayerData p = new PlayerData
                    {
                        clientId = playerObj.OwnerClientId,
                        name = name
                    };

                    if (!playersList.Contains(p))
                    {
                        playersList.Add(p);
                    }
                }
                else
                {
                    Debug.LogWarning($"PlayerObject for ClientId {playerObj.OwnerClientId} does not have a NameTag component.");
                }
            }
        }
    }
}
