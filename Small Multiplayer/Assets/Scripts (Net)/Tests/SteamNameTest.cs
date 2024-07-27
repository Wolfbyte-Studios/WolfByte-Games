using UnityEngine;
using Steamworks;

public class SteamNamePrinter : MonoBehaviour
{
    private void Start()
    {
        // Initialize Steamworks
        if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
        {
            //Application.Quit();
            return;
        }

        if (!SteamAPI.Init())
        {
            //Debug.LogError("SteamAPI_Init() failed.");
            return;
        }

        // Retrieve the Steam name
        string steamName = SteamFriends.GetPersonaName();

        // Print the Steam name
        Debug.Log("Steam Name: " + steamName);
    }

    private void OnDestroy()
    {
        // Shutdown Steamworks when the application quits
        SteamAPI.Shutdown();
    }

    private void Update()
    {
        // Run Steamworks callbacks
        //SteamAPI.RunCallbacks();
    }
}
