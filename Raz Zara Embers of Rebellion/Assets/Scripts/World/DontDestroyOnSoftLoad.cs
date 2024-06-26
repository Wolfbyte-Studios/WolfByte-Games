using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnSoftLoad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(SceneLoaderRealtime.lastLoadType == SceneLoaderRealtime.LoadType.Hard)
        {
            Destroy(this.gameObject);
        }
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
    }
}
