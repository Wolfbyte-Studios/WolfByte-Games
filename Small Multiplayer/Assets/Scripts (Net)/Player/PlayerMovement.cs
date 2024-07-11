using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float movespeed;
    public float maxVelocity;
    public bool CanFly;
    public float FlyForce = 50;
    public float jumpHeight;
    public float slowRate;



    public GameObject playerCam;
    public Rigidbody rb;
    public InputActionAsset actions;
    public InputAction move;
    public InputAction jump;
    public InputAction crouch;
    public InputAction menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = gameObject.GetComponent<Rigidbody>();

        var playerInput = gameObject.GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            move = actions.FindAction("Move");
            menu = actions.FindAction("Menu");
            menu.performed += Menu_performed;
            jump = actions.FindAction("Jump");
            jump.performed += OnJump;
            crouch = actions.FindAction("Crouch");
            crouch.performed += OnCrouch;
        }
        playerCam = gameObject.transform.FindDeepChildByTag("MainCamera").gameObject;
        playerCam.transform.parent = gameObject.transform.parent;
        if (gameObject.GetComponent<PlayerNetworkIndex>().playerIndexTarget == 0)
        {
            CanFly = true;
        }
        else
        {
            CanFly = false;
        }
    }

    private void Menu_performed(InputAction.CallbackContext obj)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext obj)
    {
        if (CanFly)
        {
            OnFly(-FlyForce);
            Debug.Log("Go down");
            return;
        }

        //Add crouch animation
        throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext obj)
    {
        if (CanFly)
        {
            OnFly(FlyForce);
            return;
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), rb.linearVelocity.z);
    }

    public void OnFly(float upDownStrength)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, upDownStrength, rb.linearVelocity.z);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void FaceCamera()
    {
        if(gameObject.GetComponent<PlayerNetworkIndex>().playerIndexTarget == 0)
        {
            gameObject.transform.eulerAngles = new Vector3(playerCam.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
            return;
        }
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
    }
    public void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        FaceCamera();

        if (move.IsPressed())
        {
            OnMove();
        }
        if (CanFly)
        {
            rb.useGravity = false;
            if (!jump.IsPressed() & !move.IsPressed() & !crouch.IsPressed())
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, slowRate);
            }
        }
        else
        {
            rb.useGravity = true;
        }

    }
    public void OnMove()
    {

        var v = move.ReadValue<Vector2>();

        // Convert the 2D input into 3D direction relative to the camera
        Vector3 forward = playerCam.transform.forward;
        Vector3 right = playerCam.transform.right;

        // We only want to move on the XZ plane, so we zero out the Y component
        forward.y = 0;
        right.y = 0;

        // Normalize to avoid faster diagonal movement
        forward.Normalize();
        right.Normalize();

        // Calculate the direction to move
        Vector3 moveDirection = (forward * v.y + right * v.x).normalized;

        // Apply the force to the Rigidbody
        rb.AddForce(moveDirection * movespeed);
        rb.maxLinearVelocity = maxVelocity;
    }
}
