using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class AimOutline : NetworkBehaviour
{
    public GameObject target;
    public InputActionAsset actions;
    public InputAction Fire;
    public InputAction Secondary;

    [SyncVar(hook = nameof(OnTargetLayerChanged))]
    private int targetLayer;

    [SyncVar(hook = nameof(OnTargetNetworkObjectIdChanged))]
    private uint targetNetworkObjectId;

    void Start()
    {
        var playerInput = gameObject.transform.parent.parent.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            Fire = actions.FindAction("Fire");
            Secondary = actions.FindAction("Secondary");
        }

        Fire.Enable();
        Secondary.Enable();
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
        Fire_performed_Command();
    }

    private void OnSecondary()
    {
        Secondary_performed_Command();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Clickable"))
        {
            if (target != null && target.gameObject != other.gameObject)
            {
                SetTargetLayer_Command(target.GetComponent<NetworkIdentity>().netId, 30);
            }

            target = other.gameObject;
            var click = target.GetComponent<Clickable>();
            int newLayer = (click.percentageFinished < 1) ? 29 : 31;
            click.OnSelect();
            SetTargetLayer_Command(target.GetComponent<NetworkIdentity>().netId, newLayer);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (target == null) return;

        if (other.gameObject.CompareTag("Clickable"))
        {
            SetTargetLayer_Command(target.GetComponent<NetworkIdentity>().netId, 30);
            var click = target.GetComponent<Clickable>();
            click.OnDeselect();
            target = null;
        }
    }

    [Command]
    private void Fire_performed_Command()
    {
        if (target == null) return;

        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerEvent();
    }

    [Command]
    private void Secondary_performed_Command()
    {
        if (target == null) return;

        var clObj = target.GetComponent<Clickable>();
        clObj.TriggerSecondary();
    }

    [Command]
    private void SetTargetLayer_Command(uint targetNetworkObjectId, int layer)
    {
        targetLayer = layer;
        this.targetNetworkObjectId = targetNetworkObjectId;
        SetTargetLayer_ClientRpc(targetNetworkObjectId, layer);
    }

    [ClientRpc]
    private void SetTargetLayer_ClientRpc(uint targetNetworkObjectId, int layer)
    {
        NetworkIdentity targetObject;
        if (NetworkServer.active)
        {
            if (NetworkServer.spawned.TryGetValue(targetNetworkObjectId, out targetObject))
            {
                targetObject.gameObject.layer = layer;
            }
        }
        else
        {
            if (NetworkClient.spawned.TryGetValue(targetNetworkObjectId, out targetObject))
            {
                targetObject.gameObject.layer = layer;
            }
        }
    }

    private void OnTargetLayerChanged(int oldLayer, int newLayer)
    {
        if (target != null)
        {
            target.layer = newLayer;
        }
    }

    private void OnTargetNetworkObjectIdChanged(uint oldId, uint newId)
    {
        NetworkIdentity newTarget;
        if (NetworkServer.active)
        {
            if (NetworkServer.spawned.TryGetValue(newId, out newTarget))
            {
                target = newTarget.gameObject;
                target.layer = targetLayer;
            }
        }
        else
        {
            if (NetworkClient.spawned.TryGetValue(newId, out newTarget))
            {
                target = newTarget.gameObject;
                target.layer = targetLayer;
            }
        }
    }
}
