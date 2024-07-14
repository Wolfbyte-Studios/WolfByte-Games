using UnityEngine;
using Unity.Netcode;

public class HoldItem : NetworkBehaviour
{
    public Transform targetLocation; // Define this in the Inspector or dynamically at runtime
    public float speed = 1.0f; // Speed at which the item will move towards the target location

    private bool isMoving = false;
    private Vector3 startPosition;
    private float startTime;
    private float journeyLength;

    void Update()
    {
        if (isMoving)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetLocation.position, fractionOfJourney);

            if (fractionOfJourney >= 1.0f)
            {
                isMoving = false; // Stop moving once the target is reached
            }
        }
    }

    [ServerRpc]
    public void Trigger_ServerRpc()
    {
        startPosition = transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPosition, targetLocation.position);
        isMoving = true;

        Trigger_ClientRpc(startPosition, startTime, journeyLength);
    }

    [ClientRpc]
    private void Trigger_ClientRpc(Vector3 startPosition, float startTime, float journeyLength)
    {
        this.startPosition = startPosition;
        this.startTime = startTime;
        this.journeyLength = journeyLength;
        isMoving = true;
    }
    public void Trigger()
    {
        Trigger_ServerRpc();
    }
}
