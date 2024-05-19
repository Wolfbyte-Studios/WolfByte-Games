using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public Transform player;
    [Header("Left/Right, Up/Down, Forward/Back")]
    public float distanceSideways;
    public float distanceAbove;
    [Range(-20f, -3f)]
    public float distanceForward;
    private Vector3 distanceFromPlayer;  // Define the offset relative to the player's orientation
    [Header("How far above the player the camera focuses")]
    [Range(0f, 3f)]
    public float LookOffset;  // Define the offset relative to the player's orientation
    [Header("Follow speed of the camera")]
    public float camSpeed;
    [Header("Look speed of the camera")]
    public float lookSpeed;
    private Vector3 target;

    void FixedUpdate()
    {
        distanceAbove = Mathf.Clamp(distanceAbove, 0.25f, (Mathf.Abs(distanceForward) * 1.5f));
        distanceForward = Mathf.Clamp(distanceForward, -20f, -3f);
        distanceSideways = Mathf.Clamp(distanceSideways, -Mathf.Abs(distanceForward), Mathf.Abs(distanceForward));
        distanceFromPlayer = new Vector3(distanceSideways,distanceAbove,distanceForward);
        // Calculate the desired position based on player position and offset
        target = player.position + player.forward * distanceFromPlayer.z + player.right * distanceFromPlayer.x + player.up * distanceFromPlayer.y;

        // Raycast to check if there's an obstacle between the player and the camera's desired position
        RaycastHit hit;
        Vector3 directionToCamera = target - player.position;
        if (Physics.Raycast(player.position, directionToCamera.normalized, out hit, directionToCamera.magnitude))
        {
            // If hit, adjust target position to be in front of the obstacle, maintaining a line of sight with the player
            target = hit.point - directionToCamera.normalized * 0.5f;  // Adjust so camera is slightly in front of the hit point
        }

        // Smoothly move the camera towards the calculated or adjusted target position
        transform.position = Vector3.Lerp(transform.position, target, camSpeed * Time.fixedDeltaTime);

        // Adjust the camera's look direction
        Vector3 lookTarget = player.position + new Vector3(0, LookOffset,0);
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.fixedDeltaTime);
    }
}
