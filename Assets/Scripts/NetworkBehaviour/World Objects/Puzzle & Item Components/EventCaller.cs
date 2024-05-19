
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class EventCaller : NetworkBehaviour
{
    public UnityEvent myEvent;
    public bool player;
    public bool activators;
    public void Start()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {

        switch (other.tag)
        {
            case "Activator":
                if (activators)
                {
                    TriggerEvent();
                }
                break;
            case "Player":
                if (player)
                {
                    TriggerEvent();
                }
                break;
            case "Player1":
                if (player)
                {
                    TriggerEvent();
                }
                break;
            case "Player2":
                if (player)
                {
                    TriggerEvent();
                }
                break;
        }
        
        
    }
    public void TriggerEvent()
    {
        if (myEvent != null)
        {
            Debug.Log("Activation should happen");
            myEvent.Invoke();
        }
    }
}
