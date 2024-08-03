using UnityEngine;
using Mirror;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine.UIElements;
using Unity.Cinemachine;
public class PlayerIdentity : NetworkBehaviour
{
    [SyncVar]
    public int playerId = 5;
    [SyncVar(hook = nameof(OnActiveChanged))]
    public bool isActiveSab = true;
    [SyncVar(hook = nameof(OnSabchanged))]
    public int sab;
    public GameObject Sab;
    public GameObject Runner;
    public GameObject playerCam;

    public override void OnStartServer()
    {
        base.OnStartServer();
        playerId = this.connectionToClient.connectionId;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkUtils.RpcHandler(this, RefreshPlayers);
    }
    
    public void Awake()
    {


        
        
        
        Sab = transform.Find("Sab").gameObject;
        Runner = transform.Find("Runner").gameObject;
        playerCam = transform.Find("PlayerCam").gameObject;
    }
    // Update is called once per frame
    public void OnSabchanged(int oldvalue, int newvalue)
    {
        NetworkUtils.RpcHandler(this, RefreshPlayers);
    }
    public void OnActiveChanged(bool oldvalue, bool newvalue)
    {
        NetworkUtils.RpcHandler(this, RefreshPlayers);
    }
    public void activatePlayerModels()
    {

        Sab.SetActive(isActiveSab);
        Runner.SetActive(!isActiveSab);
        if(isActiveSab)
        {
            playerCam.GetComponent<CinemachineCamera>().Follow = Sab.transform;
        }
        else
        {
            playerCam.GetComponent<CinemachineCamera>().Follow = Runner.transform;
        }
    }
    public void Update()
    {
        sab = GameManager.singleton.SabId;
    }
    [ContextMenu("Refresh Players")]
    public void RefreshPlayers()
    {
        

        isActiveSab = (sab == playerId);
        NetworkUtils.RpcHandler(this, activatePlayerModels);
        
    }
}
