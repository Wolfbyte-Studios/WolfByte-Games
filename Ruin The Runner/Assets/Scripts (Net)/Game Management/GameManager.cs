using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        respawnAllPlayers(false);
        //throw new System.NotImplementedException();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkUtils.RpcHandler(this, RotatePlayers);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        NetworkUtils.RpcHandler(this, RotatePlayers);

    }
    public void updateCount(int oldvalue, int newvalue)
    {
        //Debug.Log("Player count has changed!");
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
    
    public void respawnAllPlayers(bool isZeroed)
    {
        foreach(var con in NetworkServer.connections)
        {
            if (isZeroed)
            {
                con.Value.identity.gameObject.transform.position = Vector3.zero;
                con.Value.identity.gameObject.transform.GetChild(0).position = Vector3.zero;
                con.Value.identity.gameObject.transform.GetChild(1).position = Vector3.zero;
                con.Value.identity.gameObject.transform.GetChild(2).position = Vector3.zero;
                con.Value.identity.gameObject.transform.GetChild(3).position = Vector3.zero;
                return;
            }
            con.Value.identity.gameObject.GetComponentInChildren<PlayerMovement>(false).Respawn();
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
            //Debug.Log(playerObj.name);
        }
    }
    [ContextMenu("Rotate debug")]
    public void RotatePlayersDebug()
    {
        if (!isServer) return;
        var oldValue = SabId;
        //SabId = UnityEngine.Random.Range(0, NetworkServer.connections.Count);
        if (NetworkServer.connections.Count != 1)
        {
            RotatePlayers();
            return;
        }
        foreach (var conn in NetworkServer.connections)
        {
            var p = conn.Value.identity;

            var sab = p.transform.FindDeepChild("Sab").gameObject;
            var Runner = p.transform.FindDeepChild("Runner").gameObject;
            //Debug.Log(Runner.name);
            sab.SetActive(!sab.activeSelf);
            Runner.SetActive(!Runner.activeSelf);
        }
    }
}
