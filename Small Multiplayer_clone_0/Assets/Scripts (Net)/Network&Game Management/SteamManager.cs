using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private static SteamManager s_instance;
    private static bool s_EverInitialized;

    private bool m_bInitialized;
    public static bool Initialized
    {
        get
        {
            return s_instance != null && s_instance.m_bInitialized;
        }
    }

    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        //Debug.LogWarning(pchDebugText);
    }

    private void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;
        DontDestroyOnLoad(gameObject);

        if (s_EverInitialized)
        {
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        // If Steam is not running, then SteamAPI_RestartAppIfNecessary starts the
        // Steam client and also launches this game again.
        if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
        {
            Application.Quit();
            return;
        }

        // Initialize the Steamworks API.
        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized)
        {
            //Debug.LogError("SteamAPI_Init() failed.");
            return;
        }

        s_EverInitialized = true;
    }

    private void OnEnable()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    private void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }
        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    private void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
    }
}
