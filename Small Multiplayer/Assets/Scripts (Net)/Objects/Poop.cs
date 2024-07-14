using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkAnimator))]
public class Poop : NetworkBehaviour
{
    public static Transform lastPooped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lastPooped = this.transform;
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}
