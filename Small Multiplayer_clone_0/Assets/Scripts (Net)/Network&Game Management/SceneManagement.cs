using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManagement : NetworkBehaviour
{
    public static SceneManagement Instance { get; private set; }
    public List<string> Scenes = new List<string>();
    public NetworkVariable<int> SceneToLoad = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        base.OnNetworkSpawn();
        Instance = this;
        Scenes = GetScenesInBuild();
        //var totalScenes = SceneManager.sceneCountInBuildSettings;
        Scenes.RemoveAt(0);
        //uncomment when added win screen and any others at end of build scenes
        //scenesInBuild.RemoveAt(totalScenes - 1);
    }

    List<string> GetScenesInBuild()
    {
        List<string> scenesInBuild = new List<string>();
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            scenesInBuild.Add(sceneName);
        }

        return scenesInBuild;
    }
    public void ChooseScene()
    {
        var mode = CurrentSessionStats.Instance.GameMode.Value;
        switch (mode)
        {
            case CurrentSessionStats.GameModeEnum.Standard:
                SceneToLoad.Value = Random.Range(0, Scenes.Count);
                NetworkUtils.RpcHandler(this, LoadScene);
                break;
        }
    }

    public void LoadScene()
    {
        
       NetworkManager.SceneManager.LoadScene(Scenes[SceneToLoad.Value], LoadSceneMode.Single);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
