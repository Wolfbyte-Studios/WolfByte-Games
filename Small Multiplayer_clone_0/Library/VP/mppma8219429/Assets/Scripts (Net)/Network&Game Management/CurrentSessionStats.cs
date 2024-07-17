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
    public GameObject PlayerParent;
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

    public GameStateEnum GameState;
    public GameModeEnum GameMode;

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

    void Update()
    {
        if (IsServer)
        {
            UpdateServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateServerRpc()
    {
        GetPlayers();
        UpdateGame();
        UpdateGameServerRpc();
        UpdateClientRpc();
    }

    [ClientRpc]
    public void UpdateClientRpc()
    {
        GetPlayers();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateGameServerRpc()
    {
        UpdateGame();
        UpdateGameClientRpc();
    }

    [ClientRpc]
    public void UpdateGameClientRpc()
    {
        UpdateGame();
    }

    public void UpdateGame()
    {
        switch (GameMode)
        {
            case GameModeEnum.Standard:
                break;
        }

        switch (GameState)
        {
            case GameStateEnum.InGame:
                foreach (PlayerDataInspector player in playersList)
                {
                    var rigidbody = player.PlayerParent.GetComponentInChildren<Rigidbody>(false);
                    var playerMovement = player.PlayerParent.GetComponentInChildren<PlayerMovement>(false);
                    if (rigidbody != null) rigidbody.isKinematic = false;
                    if (playerMovement != null) playerMovement.enabled = true;
                }
                break;

            case GameStateEnum.UI:
                foreach (PlayerDataInspector player in playersList)
                {
                    var rigidbody = player.PlayerParent.GetComponentInChildren<Rigidbody>(false);
                    var playerMovement = player.PlayerParent.GetComponentInChildren<PlayerMovement>(false);
                    if (rigidbody != null) rigidbody.isKinematic = true;
                    if (playerMovement != null) playerMovement.enabled = false;
                }
                break;

            case GameStateEnum.Other:
                break;
        }
    }

    public void GetPlayers()
    {
        
            playersList.Clear(); // Ensure we start fresh each time we get players

            foreach (var player in nm.ConnectedClientsList)
            {
                var playerObj = player.PlayerObject;
                var nameTag = playerObj.gameObject.GetComponentInChildren<NameTag>(false);

                if (nameTag != null)
                {
                    string name = nameTag.name;
                    PlayerDataInspector p = new PlayerDataInspector
                    {
                        clientId = playerObj.OwnerClientId,
                        name = name,
                        PlayerParent = playerObj.gameObject
                    };

                    if (!playersList.Exists(pl => pl.clientId == p.clientId))
                    {
                        playersList.Add(p);
                    }
                }
                else
                {
                    Debug.LogWarning($"PlayerObject for ClientId {playerObj.OwnerClientId} does not have a NameTag component.");
                }
            }

            Debug.Log($"Updated playersList with {playersList.Count} players.");
        
    }
}
