using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTheLoadingScreen : MonoBehaviour
{
    public static int sceneToLoad;
    public static int scenesLoaded;
    public enum PlayModes {
        Single, Host, Client
    };
    public static PlayModes playMode;
    public static string IP;
    public static int Port;
    public void setMode(string p)
    {
        switch(p)
        {
            case "Single": 
                playMode = PlayModes.Single;
                break;
            case "Host":
                playMode = PlayModes.Host;
                break;
            case "Client":
                playMode = PlayModes.Client;
                break;
        }

    }
    public void LoadLoadingScreen()
    {
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }
}
