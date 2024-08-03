using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.Collections;

public class NameTag : NetworkBehaviour
{
    [SyncVar]
    public string pName; 
    public string localname;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isOwned)
        {
            pName  = PlayerPrefs.GetString("Name", "Toast");

        }
    }

    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        localname = pName .ToString();

        // Update logic (if any) goes here
    }
}
