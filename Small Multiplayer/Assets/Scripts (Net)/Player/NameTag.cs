using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.Collections;

public class NameTag : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> Name = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("FUCK"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    public string localname;

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
            Name.Value = PlayerPrefs.GetString("Name", "Toast");

        }
    }

    // Update is called once per frame
    void Update()
    {

        localname = Name.Value.ToString();

        // Update logic (if any) goes here
    }
}
