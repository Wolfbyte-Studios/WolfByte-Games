using UnityEngine;
using Unity.Netcode;

public static class NetworkUtils
{
    public delegate void RpcDelegate();
    public static void RpcHandler(NetworkBehaviour networkBehaviour, RpcDelegate Del)
    {
        if (networkBehaviour.IsServer)
        {
            CallClientRpc(networkBehaviour, Del);
        }
        else
        {
            CallServerRpc(networkBehaviour, Del);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public static void CallServerRpc(NetworkBehaviour networkBehaviour, RpcDelegate serverRpcDelegate)
    {
        //here third as client
        //here third if not server
        if (!networkBehaviour.IsServer)
        {

            serverRpcDelegate.Invoke();
        }
       
    }
    [ClientRpc(RequireOwnership = false)]
    public static void CallClientRpc(NetworkBehaviour networkBehaviour, RpcDelegate clientRpcDelegate)
    {
        //here first as server
        //here first if not server
        if (networkBehaviour.IsServer)
        {
            clientRpcDelegate.Invoke();
            CallServerRpc(networkBehaviour, clientRpcDelegate);
            //here second as server
        }
        
    }
}

