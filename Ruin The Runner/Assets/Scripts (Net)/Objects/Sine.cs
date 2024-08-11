using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class Sine : NetworkBehaviour
{
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
    }

    public TransformData MovementSettings = new TransformData { speed = 1 };
    public bool Repeating;

    private float elapsedTime = 0f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isPaused = false; // Flag to control pausing
    private Rigidbody rb; // Reference to the Rigidbody component

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (Repeating)
        {
            Trigger();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Repeating && !isPaused)
        {
            Movement();
        }
    }

    public void Trigger()
    {
        NetworkUtils.RpcHandler(this, Movement);
    }

    public void Movement()
    {
        elapsedTime += Time.deltaTime * MovementSettings.speed;

        // Calculate the new position using a sine wave pattern
        float sineValue = Mathf.Sin(elapsedTime);
        Vector3 targetPosition = initialPosition + sineValue * MovementSettings.position;

        // Calculate the new rotation using a sine wave pattern
        Quaternion targetRotation = Quaternion.Euler(
            initialRotation.eulerAngles + sineValue * MovementSettings.rotation.eulerAngles);

        // Move the Rigidbody to the new position
        rb.MovePosition(targetPosition);

        // Apply the sine wave rotation to the Rigidbody
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Abs(sineValue)));

        // If you don't want the sine wave to repeat indefinitely, add a condition to stop
        if (!Repeating && elapsedTime >= Mathf.PI * 2) // one full sine wave cycle
        {
            Repeating = false;
            elapsedTime = 0f;
        }
    }

    // Method to pause the movement
    public void Pause()
    {
        isPaused = true;
    }

    // Method to resume the movement
    public void Resume()
    {
        isPaused = false;
    }
}
