using UnityEngine;
using Mirror;
using Mirror;
using TMPro;
using System.Collections.Generic;

public class SetTextToPlayerName : NetworkBehaviour
{
    public TMP_Text label;
    public int playerIndex;
    private void Start()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        //UpdatePlayerName();
        label = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentSessionStats.Instance.netActive)
        {
            


                NetworkUtils.RpcHandler(this, UpdatePlayerName);
            
        }
    }

    private void UpdatePlayerName()
    {
        if (playerIndex >= 0 && playerIndex < CurrentSessionStats.Instance.playersList.Count)
        {
            label.text = CurrentSessionStats.Instance.playersList[playerIndex].name .ToString();
        }
        else
        {
            label.text = "";
        }
    }
}
