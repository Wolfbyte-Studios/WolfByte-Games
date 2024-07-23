using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class AimOutline : NetworkBehaviour
{
    public GameObject target;
    public InputActionAsset actions;
    public InputAction Fire;
    public InputAction Secondary;
    private NetworkVariable<int> targetLayer = new NetworkVariable<int>();
    private NetworkVariable<ulong> targetNetworkObjectId = new NetworkVariable<ulong>();

    void Start()
    {
        Fire = actions.FindAction("Fire");
        Secondary = actions.FindAction("Secondary");
        var playerInput = gameObject.transform.parent.parent.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            Fire = actions.FindAction("Fire");
            Secondary = actions.FindAction("Secondary");
        }

        targetLayer.OnValueChanged += OnTargetLayerChanged;
        targetNetworkObjectId.OnValueChanged += OnTargetNetworkObjectIdChanged;
    }

    void Update()
    {
        if (Fire.IsPressed())
        {
            OnFire();
        }

        if (Secondary.IsPressed())
        {
            OnSecondary();
        }
    }

    private void OnFire()
    {
        ////Debug.Log("Fire");
        Fire_performed_ServerRpc();
    }

    private void OnSecondary()
    {
        ////Debug.Log("Secondary");
        Secondary_performed_ServerRpc();
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
            var click = target.GetComponent<Clickable>();
            int newLayer = (click.percentageFinished < 1) ? 29 : 31;
            click.OnDeselect();
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
        var click = target.GetComponent<Clickable>();
        if (other.gameObject.tag == "Clickable")
        {
            SetTargetLayer_ServerRpc(target.GetComponent<NetworkObject>().NetworkObjectId, 30);
            click.OnDeselect();
            target = null;
        }
        else
        {
            return;
        }
    }

    [ServerRpc]
    private void Fire_performed_ServerRpc()
    {
        if (target == null) { return; }
        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerEvent();
    }

    [ServerRpc]
    private void Secondary_performed_ServerRpc()
    {
        if (target == null) { return; }
        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerSecondary();
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
