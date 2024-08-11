using UnityEngine;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    public static GameManager singleton { get; private set; }
    public enum GameState
    {
        Lobby,
        UI,
        InGame
    }
    [SyncVar]
    public GameState State = GameState.Lobby;
    public enum GameMode
    {
        Standard
    }
    [SyncVar]
    public GameMode Mode = GameMode.Standard;
    [SyncVar]
    public int SabId = 5;
    [SyncVar(hook = nameof(updateCount))]
    public int playerCount = 0;
    [SyncVar]
    public List<Poop> poopList = new List<Poop>();
    [SyncVar]
    public Transform lastPooped;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {


        singleton = this;

    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkUtils.RpcHandler(this, RotatePlayers);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkUtils.RpcHandler(this, RotatePlayers);

    }
    public void updateCount(int oldvalue, int newvalue)
    {
        Debug.Log("Player count has changed!");
        NetworkUtils.RpcHandler(this, RotatePlayers);
    }
    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
       playerCount = NetworkServer.connections.Count;
        if (poopList.Count > 1)
        {
            NetworkServer.Destroy(poopList[0].gameObject);
            poopList.RemoveAt(0);
        }
    }
    [ContextMenu("Rotate Sab")]
    public void RotatePlayers()
    {
        if (!isServer) return;
        var oldValue = SabId;
        //SabId = UnityEngine.Random.Range(0, NetworkServer.connections.Count);
        if (NetworkServer.connections.Count > 1)
        {
            Start:
            if (oldValue == SabId)
            {
               
                SabId = UnityEngine.Random.Range(0, NetworkServer.connections.Count);
                if (oldValue == SabId)
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
            var script = playerObj?.GetComponent<PlayerIdentity>();
            NetworkUtils.RpcHandler(this , script.RefreshPlayers);
            Debug.Log(playerObj.name);
        }
    }
}
