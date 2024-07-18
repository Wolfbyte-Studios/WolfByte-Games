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
// Define a struct named PlayerData
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializeByMemcpy

{
    // A unique identifier for the client
    public ulong clientId;

    // A fixed-size string for the player's name
    public FixedString64Bytes name;



    // Implementation of the IEquatable<PlayerData> interface to compare two PlayerData instances
    public bool Equals(PlayerData other)
    {
        // Check if clientId, name, and PlayerParentId are equal between this instance and the other instance
        return clientId == other.clientId && name.Equals(other.name);
    }

    // Override the Equals method to compare with an object
    public override bool Equals(object obj)
    {
        // Check if the object is a PlayerData instance and compare using the Equals method
        return obj is PlayerData other && Equals(other);
    }

    // Override the GetHashCode method to provide a hash code for this instance
    public override int GetHashCode()
    {
        // Generate a hash code using a tuple containing clientId, name, and PlayerParentId
        return (clientId, name).GetHashCode();
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        // Serialize other fields...
    }
}
[GenerateSerializationForType(typeof(PlayerData))]
public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }

    public static float CurrentNumberOfPlayers;
    public bool netActive = false;
    public float IndexOfSab;
    public float IndexOfRunner1;
    public float IndexOfRunner2;
    public float IndexOfRunner3;
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
        foreach(var player in playersList)
        {
            playernames.Add(player.name.ToString());
        }

        NetworkUtils.RpcHandler(this, GetPlayers);

            Debug.Log("I am Server");

        
    }

    /*public void UpdateGame()
    {
        
        switch (GameMode.Value)
        {
            case GameModeEnum.Standard:
                break;
        }

        switch (GameState.Value)
        {
            case GameStateEnum.InGame:
                foreach (PlayerData player in playersList)
                {
                    var playerObj = clients[(int)player.clientId].PlayerObject.gameObject;
                    
                    playerObj.GetComponentInChildren<Rigidbody>(false).isKinematic = false;
                    playerObj.GetComponentInChildren<PlayerMovement>(false).enabled = true;
                }
                break;

            case GameStateEnum.UI:
                foreach (PlayerData player in playersList)
                {
                    var playerObj = clients[(int)player.clientId].PlayerObject.gameObject;
                    Debug.Log(playerObj.ToString() + " should be frozen");
                    playerObj.GetComponentInChildren<Rigidbody>(false).isKinematic = true;
                    playerObj.GetComponentInChildren<PlayerMovement>(false).enabled = false;
                }
                break;

            case GameStateEnum.Other:
                break;
        }
    }*/
    public void GetPlayers()
    {
        if (IsServer)
        {
            
            clients.Clear();
            foreach (NetworkClient n in nm.ConnectedClientsList)
            {
                clients.Add(n);
            }


            playersList.Clear(); // Ensure we start fresh each time we get players

            foreach (var player in clients)
            {
                var playerObj = player.PlayerObject;
                Debug.Log(playerObj.name);
                var nameTag = playerObj.gameObject.GetComponentInChildren<NameTag>(false);

                if (nameTag != null)
                {
                    string name = nameTag.Name.Value.ToString();
                    PlayerData p = new PlayerData
                    {
                        clientId = playerObj.OwnerClientId,
                        name = name
                    };

                    if (!playersList.Contains(p))
                    {
                        playersList.Add(p);
                        //Debug.Log(p.name.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning($"PlayerObject for ClientId {playerObj.OwnerClientId} does not have a NameTag component.");
                }
            }



            //lastPlayer = playersList[playersList.Count - 1];
        }
    }
}