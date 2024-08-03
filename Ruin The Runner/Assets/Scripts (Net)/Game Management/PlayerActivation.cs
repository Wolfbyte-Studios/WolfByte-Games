using UnityEngine;
using Mirror;
public class PlayerActivation : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnActiveChanged))]
    public bool isActive = true;
    private PlayerIdentity PiD;
    public enum PlayerType
    {
        Sab,
        Runner
    }
    public PlayerType pType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        PiD = GetComponentInParent<PlayerIdentity>();
        RefreshPlayers();
    }

    public void OnActiveChanged(bool oldvalue, bool newvalue)
    {
        isActive = newvalue;
        this.gameObject.SetActive(isActive);
        Debug.Log("the active status of object " + gameObject.name + " should be " + isActive);
    }
    [ContextMenu("Refresh Players")]
    public void RefreshPlayers()
    {
        var sab = GameManager.singleton.SabId;
        var localId = PiD.playerId;
        
        switch (pType)
        {
            case PlayerType.Sab:
                isActive = (sab == localId);
                break;
            case PlayerType.Runner:
                isActive = ( sab != localId);
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
