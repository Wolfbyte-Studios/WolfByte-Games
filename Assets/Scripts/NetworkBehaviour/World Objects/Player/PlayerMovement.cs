using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    public Vector3 velocity;
    public float WalkSpeed;
    public float runSpeed;
    public float changeRate;
    public float rotSpeed;  // This will control the speed of the rotation
    public float jumpHeight;
    public float jumpVelocityModifier;
    public float longJumpDelay = .5f;
    public Vector3 newMoveDirection;
    public float cameraSensitivity = 1;
    public float zoomSensitivity = 1;
    [Range(0f, 180f)]
    public float angleThreshold;
    public float gravityModifier;
    public float fallAnimDelay = .5f;
    private Vector3 oldMoveDirectionDifference;
    public InputActionAsset Default;
    public InputAction jump;
    public InputAction camRotate;
    public InputAction pause;
    public InputAction move;
    public InputAction run;
    public InputAction interact;
    public InputAction strafe;
    public InputAction zoom;
    public bool isGrounded;

    public Animator animator;
    private bool isRunning = false;
    public Transform mainCameraTransform; // Reference to the main camera's transform
    private PlayerCam PlayerCam;
    public Vector3 MoveDirection;
    public bool isStrafing;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private Rigidbody rb;
    private CapsuleCollider cc;
    //Following variables are for engaging with animation events

    private void Start()
    {
        var scene = SceneManager.GetActiveScene();
        if (OwnerClientId == 0)
        {
            PlayerPrefs.SetInt("LastScene", scene.buildIndex);
            Debug.Log("Player1");
            gameObject.tag = "Player1";
        }
        if (OwnerClientId == 1)
        {
            Debug.Log("Player2");
            gameObject.tag = "Player2";
        }
        if (IsOwner)
        {
            PlayerCam = mainCameraTransform.gameObject.GetComponent<PlayerCam>();
            PlayerCam.player = gameObject.transform;
        }
        foreach (Transform pusher in transform.FindDeepChildrenByTag("Pusher"))
        {
            pusher.gameObject.SetActive(false);
        }
        cc = gameObject.GetComponent<CapsuleCollider>();
        rb = gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.parent = transform;
        groundCheck.localPosition = new Vector3(0, cc.height / 2, 0);

    }
    public void playerSpawnStuff(GameObject cam)
    {

        gameObject.transform.position = cam.transform.position;
    }
    void Awake()
    {

        magnethand = transform.FindDeepChild("Magnet").gameObject.GetComponent<MagnetismStrength>();

        animator = gameObject.GetComponent<Animator>();
        animator.applyRootMotion = false;
        jump = Default.FindAction("Jump");
        run = Default.FindAction("Run");
        camRotate = Default.FindAction("CameraRotate");
        pause = Default.FindAction("Pause");
        move = Default.FindAction("Move");
        strafe = Default.FindAction("Strafe");
        interact = Default.FindAction("Interact");
        zoom = Default.FindAction("CameraZoom");

        run.performed += _ => ToggleRun();
        mainCameraTransform = GameObject.Find("PlayerCam").transform; // Get the main camera transform

    }

    void ToggleRun()
    {
        isRunning = !isRunning; // Toggle the running state
    }
    public float timeFell;
    private bool isFalling;
    public float timeSinceLeftGround;
    public void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        velocity = rb.linearVelocity;

        var m = move.ReadValue<Vector2>();
        animator.SetBool("rootMotion", animator.applyRootMotion);
        checkGroundedStuff();

        jump.started += ctx => Jump(ctx);
        camRotate.performed += ctx => RotateCamera(ctx);
        isStrafing = strafe.IsPressed();
        zoom.performed += ctx => ZoomCamera(ctx);


        if (m != Vector2.zero)
        {
            animator.applyRootMotion = true;
            // Determine running state based on speed
            

            Move(m != Vector2.zero, isRunning, m);
        }
        else
        {
            Move(false, false, Vector2.zero);
        }

    }
    public void OnAnimatorMove()
    {
        if (isGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (animator.deltaPosition) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = rb.linearVelocity.y;
            rb.linearVelocity = v;
        }

    }
    public void checkGroundedStuff()
    {
        isGrounded = checkGrounded();
        animator.SetBool("isGrounded", isGrounded);
        timeSinceLeftGround = Time.time - timeFell;
        if (!isGrounded)
        {
            if (!isFalling)
            {
                // Player just left the ground

                timeFell = Time.time;
                animator.applyRootMotion = false;
                animator.SetBool("CanJump", false);
                if (animator.GetBool("isJumping"))
                {
                    Debug.Log("jumping, not falling");
                    return;
                }
                isFalling = true;
            }
            inAirMovement();
            // Continually check if the player has been in the air longer than fallAnimDelay
            float timeSinceLeftGround = Time.time - timeFell;
            if (timeSinceLeftGround >= fallAnimDelay)
            {
                animator.SetBool("InAir", true);
            }
        }
        else
        {
            // Player is grounded again
            if (isFalling)
            {
                isFalling = false;
                animator.applyRootMotion = true;
                animator.SetBool("InAir", false);
                animator.SetBool("CanJump", true);
                timeFell = 0; // Reset timeFell when grounded
            }

        }
    }
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public float slopeAngle;

    public Transform groundCheck;
    public bool checkGrounded()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundDistance, groundMask);
        return grounded;
    }
    public void checkSlope()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, groundDistance + 1f, groundMask))
        {
            // Calculate the angle between the ground normal and the player's forward direction
            Vector3 groundNormal = hit.normal;
            Vector3 playerForward = transform.forward;

            // Project the player's forward vector onto the plane defined by the ground normal
            Vector3 projectedForward = Vector3.ProjectOnPlane(playerForward, groundNormal);

            // Calculate the angle between the projected forward vector and the ground plane's up vector
            slopeAngle = -Vector3.SignedAngle(Vector3.up, groundNormal, transform.right);

            //Debug.Log("Slope Angle: " + slopeAngle);
        }
    }
    public void onStep()
    {
        //
    }



    public MagnetismStrength magnethand;
    [Tooltip("Strength of Magnet Hand (Somewhere around 2000)")]
    public float strength;
    public void ActivateMagnetHand()
    {
        magnethand.strength = 2000;
    }
    public void Move(bool isMoving, bool isRunning, Vector2 inputDirection)
    {
        var oldMoveDirection = newMoveDirection;
        if (!isMoving)
        {
            //animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), 0f, changeRate * Time.deltaTime));
            animator.SetFloat("Speed", 0f);
            return;
        }
        checkSlope();
        ZeroSidewaysRotation();

        // Calculate new movement direction based on camera orientation
        newMoveDirection = mainCameraTransform.forward * inputDirection.y + mainCameraTransform.right * inputDirection.x;
        newMoveDirection.y = 0; // Ensure the movement is purely horizontal
        newMoveDirection.Normalize(); // Normalize to get direction only
        MoveDirection = newMoveDirection;

        // Calculate the angle and direction between the old and the new movement direction
        float turnAngle = Vector3.Angle(oldMoveDirection, newMoveDirection);
        Vector3 crossProduct = Vector3.Cross(oldMoveDirection, newMoveDirection);

        // Check if the turn is clockwise or counterclockwise
        bool isClockwise = crossProduct.y < 0;

        // If the character has turned around more than a certain angle, trigger the turning animation
        if (turnAngle > angleThreshold) // You can adjust this threshold based on what feels right in your game
        {
            if (isClockwise)
                animator.SetFloat("Rotation", 10);
            else
                animator.SetFloat("Rotation", -10);
        }
        else
        {
            animator.SetFloat("Rotation", 0);
        }

        // Continue with the usual movement logic
        Quaternion targetRotation = Quaternion.LookRotation(newMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed); // Smoothly rotate towards the target direction

        float targetSpeed = isRunning ? runSpeed : WalkSpeed;
        float currentSpeed = animator.GetFloat("Speed");

        if (isRunning)
        {
            animator.SetFloat("Speed", Mathf.Clamp(currentSpeed + changeRate * inputDirection.magnitude, -targetSpeed, targetSpeed));
        }
        else
        {
            animator.SetFloat("Speed", Mathf.Lerp(currentSpeed, 1f, 2 * changeRate * Time.deltaTime));
        }
    }

    public void RotateCamera(InputAction.CallbackContext context)
    {
        Vector2 rotation = context.ReadValue<Vector2>();

        switch (isStrafing)
        {
            //Normal Camera behaviour
            case false:
                //up/down
                PlayerCam.distanceAbove += rotation.normalized.y * cameraSensitivity * .5f;

                //left/right
                PlayerCam.distanceSideways += rotation.normalized.x * cameraSensitivity * .5f;


                break;
            //Camera behaviour while strafing
            case true:

                break;
        }

    }
    public void inAirMovement()
    {
        Vector3 v = MoveDirection * jumpVelocityModifier * (animator.GetFloat("Speed") / runSpeed);
        v.y = rb.linearVelocity.y;
        rb.linearVelocity = v;
    }
    float CalculateInitialVelocity(float height, float gravity)
    {
        return Mathf.Sqrt(-2 * gravity * height);
    }
    public void ZeroSidewaysRotation()
    {
        PlayerCam.distanceSideways = 0;
    }
    public void ZoomCamera(InputAction.CallbackContext context)
    {
        float zoom = context.ReadValue<float>();
        // For zoom, use later
        PlayerCam.distanceForward += zoom * zoomSensitivity;
    }
    private float timeJumped;
    public float jumpDelay;
    public void Jump(InputAction.CallbackContext context)
    {
        
        if (Time.time - timeJumped < jumpDelay)
        {
            return;
        }
        // Disable root motion for the jump
        if (animator.GetBool("CanJump") == false)
        {
            return;
        }

        timeJumped = Time.time;
        if (animator.GetFloat("Speed") >= runSpeed)
        {

            StartCoroutine(longJump());
            return;
        }
        var vel = rb.linearVelocity;
        animator.applyRootMotion = false;
        animator.SetBool("CanJump", true);
        // Store the current horizontal velocity


        // Set the new velocity with the calculated vertical component
        animator.SetTrigger("Jump");
        rb.linearVelocity = vel + (Vector3.up * CalculateInitialVelocity(jumpHeight, Physics.gravity.y) + (Vector3.forward * jumpVelocityModifier));
        animator.SetBool("isGrounded", false);
        
        isGrounded = false;


        Debug.Log(rb.linearVelocity + " " + CalculateInitialVelocity(jumpHeight, Physics.gravity.y));
       
    }
    IEnumerator longJump()
    {


        // Store the current horizontal velocity


        // Set the new velocity with the calculated vertical component


        animator.SetTrigger("Jump");
        animator.SetBool("isJumping", true);
        yield return new WaitForSeconds(longJumpDelay);
        animator.SetBool("isGrounded", false);

        var vel = rb.linearVelocity;
        animator.applyRootMotion = false;
        rb.linearVelocity = vel + (Vector3.up * CalculateInitialVelocity(4f, Physics.gravity.y));

        isGrounded = false;


        Debug.Log(rb.linearVelocity + " " + CalculateInitialVelocity(jumpHeight, Physics.gravity.y));

        yield return new WaitForSeconds(longJumpDelay);
        animator.SetBool("isJumping", false);
        yield return null;
    }
    public void Interact()
    {
        // Interaction logic here
    }
    public void Push(GameObject pushable)
    {

    }

    void OnNetworkDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (run != null) run.performed -= _ => ToggleRun();
        if (jump != null) jump.started -= Jump;
        if (camRotate != null) camRotate.performed -= RotateCamera;
        if (zoom != null) zoom.performed -= ZoomCamera;
    }
}
