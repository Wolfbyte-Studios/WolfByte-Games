using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;

using Unity.Collections;



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
    public static float IndexOfSab;
    public static float IndexOfRunner1;
    public static float IndexOfRunner2;
    public static float IndexOfRunner3;
    public UnityTransport transport;
    public NetworkManager nm;

    public JoinAndHost joinAndHost;

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
        if (IsServer)
        {
            CallMethodClientRpc("setnames");
        }
        if (IsClient)
        {
            CallMethodServerRpc("setnames");
        }
    }

    void Start()
    {
        nm = NetworkManager.Singleton;
        transport = nm.GetComponent<UnityTransport>();
        playersList = new List<PlayerDataInspector>();
        joinAndHost = FindFirstObjectByType<JoinAndHost>();

        if (IsServer)
        {

        }
    }


    public void setnames()
    {

    }
    [ClientRpc]
    public void CallMethodClientRpc(string methodName)
    {
        // Use reflection to find the method by name and invoke it
        var method = GetType().GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(this, null);
        }
        else
        {
            Debug.LogWarning("Method not found: " + methodName);
        }
    }
    [ServerRpc]
    public void CallMethodServerRpc(string methodName)
    {
        // Use reflection to find the method by name and invoke it
        var method = GetType().GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(this, null);
        }
        else
        {
            Debug.LogWarning("Method not found: " + methodName);
        }
        CallMethodClientRpc(methodName);
    }

}
