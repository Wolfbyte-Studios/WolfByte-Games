using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneStuff : NetworkBehaviour
{
    public static SceneStuff Instance { get; private set; }
    public List<string> Scenes = new List<string>();
    [SyncVar]
    public int SceneToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        base.OnStartClient();
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
        var mode = GameManager.singleton.Mode;
        switch (mode)
        {
            case GameManager.GameMode.Standard:
                SceneToLoad  = Random.Range(0, Scenes.Count);
                LoadScene();
                break;
        }
    }

    public void LoadScene()
    {
        
       SceneManager.LoadScene( Scenes[SceneToLoad ], LoadSceneMode.Single);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
