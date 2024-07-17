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
    public float IndexOfSab;
    public float IndexOfRunner1;
    public float IndexOfRunner2;
    public float IndexOfRunner3;
    public UnityTransport transport;
    public NetworkManager nm;
    public GameObject playertemp;

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
    public NetworkVariable<GameModeEnum> GameMode = new NetworkVariable<GameModeEnum>();
    public NetworkVariable<GameStateEnum> GameState = new NetworkVariable<GameStateEnum>();
    public NetworkList<PlayerData> playersList = new NetworkList<PlayerData>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        nm = NetworkManager.Singleton;
        Instance = this;
    }
    public void Update()
    {
        if (IsServer)
        {

            GetPlayers();
            UpdateGame();
            Debug.Log("I am Server");
            
        }
    }
    public void UpdateGame()
    {
        switch (GameMode.Value)
        {
            case GameModeEnum.Standard:
                break;
        }

        switch (GameState.Value)
        {
            case GameStateEnum.InGame:
                break;

            case GameStateEnum.UI:
                foreach (PlayerData player in playersList)
                {
                    var playerObj = nm.ConnectedClientsList[(int)player.clientId].PlayerObject.gameObject;
                    playerObj.GetComponentInChildren<Rigidbody>().isKinematic = true;
                    playerObj.GetComponentInChildren<PlayerMovement>().enabled = false;
                }
                break;

            case GameStateEnum.Other:
                break;
        }
    }
    public void GetPlayers()
    {
        if (IsServer)
        {
            playersList.Clear(); // Ensure we start fresh each time we get players

            foreach (var player in nm.ConnectedClientsList)
            {
                var playerObj = player.PlayerObject;
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
                        Debug.Log(p.name.ToString());
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