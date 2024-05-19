using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public Transform player;
    [Header("Up/Down, Forward/Backward, Sideways")]
    public Vector3 distanceFromPlayer;  // Define the offset relative to the player's orientation
    [Header("Follow speed of the camera")]
    public float lightSpeed;
    public string tagg;

    private Vector3 target;

    private void Start()
    {
        // Find player by tag, you should already have a Player tag set on your player GameObject
        switch (gameObject.tag)
        {
            case "MainCamera":
                player = transform.parent.FindDeepChildByTag("Player");
                break;
            case "CameraTarget":
                player = transform.parent;
                break;
        }
    }
    public void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }


        // Calculate the target position based on the player's orientation
        // player.forward gives the forward direction of the player
        // player.right gives the right direction of the player
        // player.up gives the upward direction of the player
        target = player.position +
                 player.forward * distanceFromPlayer.z +
                 player.right * distanceFromPlayer.x +
                 player.up * distanceFromPlayer.y;
        // Move towards the target position using Lerp for smooth following
        transform.position = Vector3.Lerp(transform.position, target, lightSpeed * Time.fixedDeltaTime);
    }
}
