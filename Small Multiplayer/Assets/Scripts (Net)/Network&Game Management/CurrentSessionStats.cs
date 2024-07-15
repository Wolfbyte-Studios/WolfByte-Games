using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;

using Unity.Collections;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString64Bytes name;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
    }

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
        unchecked
        {
            int hashCode = clientId.GetHashCode();
            hashCode = (hashCode * 397) ^ name.GetHashCode();
            return hashCode;
        }
    }
}

[Serializable]
public struct PlayerDataInspector
{
    public ulong clientId;
    public string name;

    public PlayerDataInspector(ulong clientId, FixedString64Bytes name)
    {
        this.clientId = clientId;
        this.name = name.ToString();
    }
}

public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }

    public static float CurrentNumberOfPlayers;
    public static float IndexOfSab;
    public static float IndexOfRunner1;
    public static float IndexOfRunner2;
    public static float IndexOfRunner3;
    public UnityTransport transport;
    public NetworkManager nm;
    public NetworkList<PlayerData> players;
    public JoinAndHost joinAndHost;

    [SerializeField]
    public List<PlayerDataInspector> playersListInspector = new List<PlayerDataInspector>();

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

    void Start()
    {
        nm = NetworkManager.Singleton;
        transport = nm.GetComponent<UnityTransport>();
        players = new NetworkList<PlayerData>();
        joinAndHost = FindFirstObjectByType<JoinAndHost>();

        if (IsServer)
        {
            players.OnListChanged += HandlePlayersListChanged;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            players.Clear();
            foreach (var player in nm.ConnectedClients)
            {
                FixedString64Bytes playerName = "Unknown";
                if (joinAndHost.playerNames.TryGetValue(player.Key, out string name))
                {
                    playerName = name;
                }
                players.Add(new PlayerData { clientId = player.Key, name = playerName });
            }
        }

        // Update the inspector list with the current players
        playersListInspector.Clear();
        foreach (var player in players)
        {
            playersListInspector.Add(new PlayerDataInspector(player.clientId, player.name));
        }
    }

    private void HandlePlayersListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        // Handle the player list change event if needed
    }
}
