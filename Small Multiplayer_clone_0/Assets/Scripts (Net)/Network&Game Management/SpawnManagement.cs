using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
public class SpawnManagement : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted; ;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {

        NetworkUtils.RpcHandler(this, updatePostitions);
        StartGame();
    }

    public List<GameObject> players;
    public void updatePostitions()
    {
        var pedestalParent = GameObject.Find("Player List");


        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {

            if (!players.Contains(g))
            {
                players.Add(g);
            }


        }

        foreach (var player in players)
        {



            //Debug.Log(player.name);
            int id = (int)player.transform.parent.GetComponent<NetworkObject>().OwnerClientId;
            var SpawnPoint = this.gameObject;

           

            player.transform.position = SpawnPoint.transform.position;
            //player.transform.localEulerAngles = new Vector3(0, 180, 0);
        }
    }
    public void StartGame()
    {
        CurrentSessionStats.Instance.GameState.Value = CurrentSessionStats.GameStateEnum.InGame;
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {

        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
