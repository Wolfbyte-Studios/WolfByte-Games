using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderRealtime : MonoBehaviour
{
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
    public List<string> loadedScenes = new List<string>();

    void Start()
    {
        finder = gameObject.GetComponent<SceneObjectFinder>();
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(p);
        }
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode scenemode)
    {
        Spawn = GameObject.Find(finder.spawnPoints[finder.selectedSpawnPointIndex]).transform;
        foreach (GameObject p in players)
        {
            p.GetComponent<CharacterController>().enabled = false;
            p.GetComponent<Animator>().enabled = false;
            p.transform.position = Spawn.position;
            p.GetComponent<CharacterController>().enabled = true;
            p.GetComponent<Animator>().enabled = true;
        }
    }

    void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            var scene = SceneManager.GetSceneByName(finder.sceneToLoad);
            LoadScene(loadType, scene);
        }
    }

    public void LoadScene(LoadType loadType, Scene scene)
    {
        switch (loadType)
        {
            case LoadType.Soft:
                lastLoadType = LoadType.Soft;

                if (scene.IsValid())
                {
                    Debug.LogWarning("Already loaded");
                    return;
                }

                loadedScenes.Add(SceneManager.GetActiveScene().name);

                SceneManager.LoadScene(finder.sceneToLoad, LoadSceneMode.Additive);

                if (loadedScenes.Count > 2)
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadedScenes[loadedScenes.Count - 1]));
                }

                if (loadedScenes.Count > 2)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadedScenes[0]));
                    loadedScenes.RemoveAt(0);
                }
                return;

            case LoadType.Hard:
                lastLoadType = LoadType.Soft;

                if (scene.IsValid())
                {
                    Debug.LogWarning("Already loaded");
                    return;
                }

                loadedScenes.Add(SceneManager.GetActiveScene().name);

                SceneManager.LoadScene(finder.sceneToLoad, LoadSceneMode.Additive);

                if (loadedScenes.Count > 2)
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadedScenes[loadedScenes.Count - 1]));
                }

                if (loadedScenes.Count > 2)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadedScenes[0]));
                    loadedScenes.RemoveAt(0);
                }
                return;
        }
    }
}
