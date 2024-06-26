using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderRealtime : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SceneObjectFinder finder;
    public static Transform Spawn;
    public List<GameObject> players = new List<GameObject>();
    public static LoadType lastLoadType;
    public enum LoadType
    {
        Soft,
        Hard
    }
    public LoadType loadType;
    public List<SceneAsset> loadedScenes = new List<SceneAsset>();
    void Start()
    {
        finder = gameObject.GetComponent<SceneObjectFinder>();
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(p);
        }
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        //loadedScenes.Add(GetSceneAsset(SceneManager.GetActiveScene()));
        
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode scenemode)
    {
        Spawn = GameObject.Find(finder.spawnPoints[finder.selectedSpawnPointIndex]).transform;
        foreach (GameObject p in players)
        {
            p.transform.gameObject.GetComponent<CharacterController>().enabled = false;
            p.transform.gameObject.GetComponent<Animator>().enabled = false;
            p.transform.position = Spawn.position;
            p.transform.gameObject.GetComponent<CharacterController>().enabled = true;
            p.transform.gameObject.GetComponent<Animator>().enabled = true;

            
        }
        return;
        throw new NotImplementedException();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            var scene = SceneManager.GetSceneByName(finder.sceneToLoad.name);
            LoadScene(loadType, scene);
        }
    }
    public void LoadScene(LoadType loadType, Scene scene)
    {
        switch (loadType)
        {
            case LoadType.Soft:
                lastLoadType = LoadType.Soft;
                //reference list of scenes, assuming current scene is primary
                
                if (scene.IsValid())
                {
                    Debug.LogWarning("Already loaded");
                    return;
                }
                
                loadedScenes.Add(GetSceneAsset(SceneManager.GetActiveScene()));

                //load new scene additively
                
                SceneManager.LoadScene(finder.sceneToLoad.name, LoadSceneMode.Additive);

                //set new scene to primary
                if(loadedScenes.Count > 2)
                {

                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadedScenes[loadedScenes.Count - 1].name));
                }
                //unload oldest scene
                if(loadedScenes.Count > 2)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadedScenes[0].name));
                    loadedScenes.RemoveAt(0);   
                }
                //get gameobect selected to teleport to
                
                //teleport players
                
                return;
            case LoadType.Hard:
                lastLoadType = LoadType.Soft;
                //reference list of scenes, assuming current scene is primary
                
                if (scene.IsValid())
                {
                    Debug.LogWarning("Already loaded");
                    return;
                }
                
                loadedScenes.Add(GetSceneAsset(SceneManager.GetActiveScene()));

                //load new scene additively
                
                SceneManager.LoadScene(finder.sceneToLoad.name, LoadSceneMode.Additive);

                //set new scene to primary
                if(loadedScenes.Count > 2)
                {

                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadedScenes[loadedScenes.Count - 1].name));
                }
                //unload oldest scene
                if(loadedScenes.Count > 2)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadedScenes[0].name));
                    loadedScenes.RemoveAt(0);   
                }
                return;
        }
    }
    public static SceneAsset GetSceneAsset(Scene scene)
    {
        if (!scene.IsValid())
        {
            Debug.LogError("Invalid Scene");
            return null;
        }

        string scenePath = scene.path;
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Scene path is empty");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
    }
}
