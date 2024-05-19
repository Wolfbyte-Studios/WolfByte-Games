using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraLook : NetworkBehaviour
{
    public GameObject player;
    public Vector3 offset;
    public Transform target;
    public string tagg;
    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = transform.parent.FindDeepChildByTag("Player").gameObject;
        }
        if (target == null)
        {
            target = player.transform.FindDeepChildByTag("CameraTarget").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        gameObject.transform.LookAt(target);
    }
}
