using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AimOutline : NetworkBehaviour
{
    public GameObject target;
    public InputActionAsset actions;
    public InputAction Fire;
    private NetworkVariable<int> targetLayer = new NetworkVariable<int>();
    private NetworkVariable<ulong> targetNetworkObjectId = new NetworkVariable<ulong>();

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

        targetLayer.OnValueChanged += OnTargetLayerChanged;
        targetNetworkObjectId.OnValueChanged += OnTargetNetworkObjectIdChanged;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FixedUpdate()
    {
        if (Fire.IsPressed())
        {
            OnFire();
        }
    }

    public void OnFire()
    {
        Debug.Log("Fire");
        Fire_performed_ServerRpc();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Clickable")
        {
            if (target != null && target.gameObject != other.gameObject)
            {
                SetTargetLayer_ServerRpc(target.GetComponent<NetworkObject>().NetworkObjectId, 30);
            }

            target = other.gameObject;

            int newLayer = (target.GetComponent<Clickable>().percentageFinished < 1) ? 29 : 31;
            SetTargetLayer_ServerRpc(target.GetComponent<NetworkObject>().NetworkObjectId, newLayer);
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
            SetTargetLayer_ServerRpc(target.GetComponent<NetworkObject>().NetworkObjectId, 30);
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
        if (target == null) { return; }
        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerEvent();
    }

    [ServerRpc]
    private void SetTargetLayer_ServerRpc(ulong targetNetworkObjectId, int layer)
    {
        targetLayer.Value = layer;
        this.targetNetworkObjectId.Value = targetNetworkObjectId;
        SetTargetLayer_ClientRpc(targetNetworkObjectId, layer);
    }

    [ClientRpc]
    private void SetTargetLayer_ClientRpc(ulong targetNetworkObjectId, int layer)
    {
        var targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        targetObject.gameObject.layer = layer;
    }

    private void OnTargetLayerChanged(int oldLayer, int newLayer)
    {
        if (target != null)
        {
            target.layer = newLayer;
        }
    }

    private void OnTargetNetworkObjectIdChanged(ulong oldId, ulong newId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newId, out var newTarget))
        {
            target = newTarget.gameObject;
            target.layer = targetLayer.Value;
        }
    }
}
