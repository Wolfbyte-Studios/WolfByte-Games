using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AimOutline : NetworkBehaviour
{
    public GameObject target;
    public InputActionAsset actions;
    public InputAction Fire;

    void Start()
    {
        Fire = actions.FindAction("Fire");
        var playerInput = gameObject.transform.parent.parent.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            Fire = actions.FindAction("Fire");
            //Fire.performed += OnFire;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FixedUpdate()
    {
        if(Fire.IsPressed())
        {
            OnFire();
        }
    }
    public void OnFire()
    {
        Debug.Log("Fuck");
        Fire_performed_ServerRpc();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Clickable")
        {
            target = other.gameObject;
            target.layer = 31;
        }
        else
        {
            return;
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        if (target == null) { return; }
        if (other.gameObject.tag == "Clickable")
        {
            target.layer = 30;
            target = null;
        }
        else
        {
            return;
        }
    }
    private void OnNetworkInstantiate()
    {
        
    }
    [ServerRpc]
    private void Fire_performed_ServerRpc()
    {
        if(target == null) { return; }
        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerEvent();
        
    }
}
