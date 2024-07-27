using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class SpawnManagement : NetworkBehaviour
{
    public List<GameObject> players = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to the scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the scene load event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isServer)
        {
            StartCoroutine(HandleSceneLoad());
        }
    }

    private IEnumerator<WaitForSeconds> HandleSceneLoad()
    {
        // Wait a short time to ensure all clients have loaded the scene
        yield return new WaitForSeconds(1.0f);

        RpcUpdatePositions();
        StartGame();
    }

    [ClientRpc]
    public void RpcUpdatePositions()
    {
        var pedestalParent = GameObject.Find("Player List");

        // Ensure players list is clear before populating
        players.Clear();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!players.Contains(g))
            {
                players.Add(g);
            }
        }

        foreach (var player in players)
        {
            // Assuming that player GameObjects are correctly set up with NetworkIdentity
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            if (identity != null)
            {
                int id = (int)identity.connectionToClient.connectionId;

                // Assigning the player's spawn point
                var spawnPoint = pedestalParent.transform.GetChild(id % pedestalParent.transform.childCount);

                player.transform.position = spawnPoint.position;
                player.transform.localEulerAngles = new Vector3(0, 180, 0);
            }
        }
    }

    public void StartGame()
    {
        // Assuming CurrentSessionStats is correctly set up
        CurrentSessionStats.Instance.GameState = CurrentSessionStats.GameStateEnum.InGame;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
