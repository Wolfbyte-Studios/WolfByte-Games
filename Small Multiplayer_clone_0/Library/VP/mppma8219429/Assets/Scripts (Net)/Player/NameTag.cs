using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.Collections;

public class NameTag : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> pName = new NetworkVariable<FixedString128Bytes>(default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    public string localname;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            pName.Value = PlayerPrefs.GetString("Name", "Toast");

        }
    }

    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        localname = pName.Value.ToString();

        // Update logic (if any) goes here
    }
}
