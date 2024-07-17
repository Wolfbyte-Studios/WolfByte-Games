using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.Collections;

[GenerateSerializationForType(typeof(string))]
public class NameTag : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> Name = new NetworkVariable<FixedString64Bytes>();
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
            localname = Name.Value.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic (if any) goes here
    }
}
