using UnityEngine;
using Mirror;
using System.Collections;

public static class NetworkUtils
{
    public delegate void RpcDelegate();
    public delegate IEnumerator RpcCoroutineDelegate();

    public static void RpcHandler(NetworkBehaviour networkBehaviour, RpcDelegate Del)
    {
        if (networkBehaviour.isServer)
        {
            networkBehaviour.RpcCallClientRpc(Del);
        }
        else
        {
            networkBehaviour.CmdCallServerRpc(Del);
        }
    }

    public static void RpcCoroutineHandler(NetworkBehaviour networkBehaviour, RpcCoroutineDelegate Del)
    {
        if (networkBehaviour.isServer)
        {
            networkBehaviour.StartCoroutine(ClientRpcCoroutine(networkBehaviour, Del));
        }
        else
        {
            networkBehaviour.CmdCallServerRpcCoroutineMethod(Del);
        }
    }

    [ClientRpc]
    private static void RpcCallClientRpc(this NetworkBehaviour networkBehaviour, RpcDelegate clientRpcDelegate)
    {
        if (!networkBehaviour.isServer)
        {
            clientRpcDelegate.Invoke();
        }
        else
        {
            networkBehaviour.CmdCallServerRpc(clientRpcDelegate);
        }
    }

    [Command]
    private static void CmdCallServerRpc(this NetworkBehaviour networkBehaviour, RpcDelegate serverRpcDelegate)
    {
        if (networkBehaviour.isServer)
        {
            serverRpcDelegate.Invoke();
        }
        else
        {
            networkBehaviour.RpcCallClientRpc(serverRpcDelegate);
        }
    }

    [Command]
    private static void CmdCallServerRpcCoroutineMethod(this NetworkBehaviour networkBehaviour, RpcCoroutineDelegate serverRpcCoroutineDelegate)
    {
        if (networkBehaviour.isServer)
        {
            networkBehaviour.StartCoroutine(serverRpcCoroutineDelegate());
        }
        else
        {
            networkBehaviour.RpcCallClientRpcCoroutineMethod(serverRpcCoroutineDelegate);
        }
    }

    [ClientRpc]
    private static void RpcCallClientRpcCoroutineMethod(this NetworkBehaviour networkBehaviour, RpcCoroutineDelegate clientRpcCoroutineDelegate)
    {
        if (!networkBehaviour.isServer)
        {
            networkBehaviour.StartCoroutine(clientRpcCoroutineDelegate());
        }
        else
        {
            networkBehaviour.StartCoroutine(ClientRpcCoroutine(networkBehaviour, clientRpcCoroutineDelegate));
        }
    }

    private static IEnumerator ServerRpcCoroutine(NetworkBehaviour networkBehaviour, RpcCoroutineDelegate coroutineDelegate)
    {
        if (!networkBehaviour.isServer)
        {
            yield return networkBehaviour.StartCoroutine(coroutineDelegate());
        }
    }

    private static IEnumerator ClientRpcCoroutine(NetworkBehaviour networkBehaviour, RpcCoroutineDelegate coroutineDelegate)
    {
        if (networkBehaviour.isServer)
        {
            yield return networkBehaviour.StartCoroutine(coroutineDelegate());
            yield return networkBehaviour.StartCoroutine(ServerRpcCoroutine(networkBehaviour, coroutineDelegate));
        }
    }
}
