using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEditor.Build;

[Serializable]
public struct PlayerDataInspector
{
    public ulong clientId;
    public string name;
    public GameObject PlayerParent;
}
[GenerateSerializationForTypeAttribute(typeof(PlayerDataInspector))]
public class CurrentSessionStats : NetworkBehaviour
{
    public static CurrentSessionStats Instance { get; private set; }

    public static float CurrentNumberOfPlayers;
    public float IndexOfSab;
    public float IndexOfRunner1;
    public float IndexOfRunner2;
    public float IndexOfRunner3;
    public UnityTransport transport;
    public NetworkManager nm = NetworkManager.Singleton;
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

    public NetworkVariable<GameStateEnum> GameState = new NetworkVariable<GameStateEnum>(GameStateEnum.UI);
    public NetworkVariable<GameModeEnum> GameMode = new NetworkVariable<GameModeEnum>(GameModeEnum.Standard);


    [NonSerialized]
    public NetworkVariable<List<PlayerDataInspector>> playersList = new NetworkVariable<List<PlayerDataInspector>>();
    public List<PlayerDataInspector> PlayersListInspector;

   
}
