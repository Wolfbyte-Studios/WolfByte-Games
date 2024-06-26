using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderRealtime : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SceneObjectFinder finder;
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
                //reference list of scenes, assuming current scene is primary
                loadedScenes.Add(GetSceneAsset(SceneManager.GetActiveScene()));

                //load new scene additively
                SceneManager.LoadScene(finder.sceneToLoad.name, LoadSceneMode.Additive);

                //set new scene to primary
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadedScenes[2].name));
                //unload oldest scene
                if(loadedScenes.Count > 2)
                {
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(loadedScenes[0].name));
                    loadedScenes.RemoveAt(0);   
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
