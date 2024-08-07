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

        // Retrieve the SteamID64
        ulong steamID64 = SteamUser.GetSteamID().m_SteamID;

        // Print the Steam name and SteamID64
        Debug.Log("Steam Name: " + steamName);
        Debug.Log("SteamID64: " + steamID64);
    }

    private void OnDestroy()
    {
        // Shutdown Steamworks when the application quits
        SteamAPI.Shutdown();
    }

    private void Update()
    {
        // Run Steamworks callbacks
        SteamAPI.RunCallbacks();
    }
}
