using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loadscreen : MonoBehaviour
{
    // Start is called before the first frame update
    
    void Start()

    {
        LoadTheLoadingScreen.sceneToLoad = PlayerPrefs.GetInt("LastScene", 1);
        StartCoroutine(LoadYourAsyncScene());
    }

    // Update is called once per frame
    void Update()
    {


     
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.






            // Then load the target scene
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(LoadTheLoadingScreen.sceneToLoad, LoadSceneMode.Single);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
            // Here you can update your progress bar or do other work
            
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                    PlayerPrefs.SetInt("ActiveScenes", SceneManager.sceneCount);
                }

                yield return null;
            }
        
        
    }




}
