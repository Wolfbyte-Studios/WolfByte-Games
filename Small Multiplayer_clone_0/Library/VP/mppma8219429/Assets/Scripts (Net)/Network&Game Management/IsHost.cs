using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;

public class IsHost : NetworkBehaviour
{
    public enum forHostOrClient
    {
        Host,
        Client
    };
    public forHostOrClient ForWho;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        switch (ForWho)
        {
            case forHostOrClient.Host:
                if(!IsHost)
                {
                    this.gameObject.SetActive(false);
                }
                return;
                case forHostOrClient.Client:
                if (!IsClient)
                {
                    this.gameObject.SetActive(false);
                }
                return;
        }
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}
