using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;

public class NameTag : NetworkBehaviour
{
    public string Name;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Start();
    }
    public void Start()
    {
        if (IsOwner)
        {
            name = PlayerPrefs.GetString(Name, "Toast");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
