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
            CallClientRpc(networkBehaviour, Del);
        }
        else
        {
            CallServerRpc(networkBehaviour, Del);
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
            networkBehaviour.StartCoroutine(ServerRpcCoroutine(networkBehaviour, Del));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public static void CallServerRpc(NetworkBehaviour networkBehaviour, RpcDelegate serverRpcDelegate)
    {
        if (!networkBehaviour.isServer)
        {
            serverRpcDelegate.Invoke();
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public static void CallClientRpc(NetworkBehaviour networkBehaviour, RpcDelegate clientRpcDelegate)
    {
        if (networkBehaviour.isServer)
        {
            clientRpcDelegate.Invoke();
            CallServerRpc(networkBehaviour, clientRpcDelegate);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public static void CallServerRpcCoroutineMethod(NetworkBehaviour networkBehaviour, RpcCoroutineDelegate serverRpcCoroutineDelegate)
    {
        if (!networkBehaviour.isServer)
        {
            networkBehaviour.StartCoroutine(serverRpcCoroutineDelegate());
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public static void CallClientRpcCoroutineMethod(NetworkBehaviour networkBehaviour, RpcCoroutineDelegate clientRpcCoroutineDelegate)
    {
        if (networkBehaviour.isServer)
        {
            networkBehaviour.StartCoroutine(clientRpcCoroutineDelegate());
            networkBehaviour.StartCoroutine(ServerRpcCoroutine(networkBehaviour, clientRpcCoroutineDelegate));
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
