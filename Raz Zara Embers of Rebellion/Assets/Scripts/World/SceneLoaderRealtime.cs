using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderRealtime : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SceneObjectFinder finder;
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
        //loadedScenes.Add(GetSceneAsset(SceneManager.GetActiveScene()));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            LoadScene(loadType);
        }
    }
    public void LoadScene(LoadType loadType)
    {
        switch (loadType)
        {
            case LoadType.Soft:
                lastLoadType = LoadType.Soft;
                //reference list of scenes, assuming current scene is primary
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
                foreach (GameObject p in players)
                {
                    p.transform.position = GameObject.Find(finder.spawnPoints[finder.selectedSpawnPointIndex]).transform.position;
                }
                return;
            case LoadType.Hard:
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
