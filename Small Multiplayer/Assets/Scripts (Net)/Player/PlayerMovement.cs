using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public float maxVelocity;
    public bool CanFly;
    public float FlyForce = 50;
    public float jumpHeight;
    public float slowRate;
    

    public bool Dizzy = false;

    public GameObject playerCam;
    public Rigidbody rb;
    public InputActionAsset actions;
    public InputAction move;
    public InputAction jump;
    public InputAction crouch;
    public InputAction menu;
    public Animator anim;
    private PlayerNetworkIndex PLI;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Setup();
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);

    }
    public void OnEnable()
    {
        Setup();
    }
    public void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Setup()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = gameObject.GetComponent<Rigidbody>();

        var playerInput = gameObject.GetComponent<PlayerInput>();
        PLI = gameObject.GetComponent<PlayerNetworkIndex>();

        if (playerInput != null)
        {
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            move = actions.FindAction("Move");
            menu = actions.FindAction("Menu");
            menu.performed += Menu_performed;
            jump = actions.FindAction("Jump");
            crouch = actions.FindAction("Crouch");
            crouch.performed += OnCrouch;
        }
        playerCam = PLI.playerCamera.gameObject;
        playerCam.transform.parent = gameObject.transform.parent;
        if (PLI.PlayerType == PlayerNetworkIndex.playerType.Sab)
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

    }

    public void OnCrouch(InputAction.CallbackContext obj)
    {
        if (CanFly)
        {
            OnFly(-FlyForce);
            //Debug.Log("Go down");
            return;
        }

        //Add crouch animation
        //throw new System.NotImplementedException();
    }

    public void OnJump()
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
        Vector3 velocity = rb.linearVelocity;
        velocity.y = upDownStrength;
        rb.linearVelocity = velocity;
    }

    void Update()
    {
        
    }

    public void FaceCamera()
    {
        if (PLI.PlayerType == PlayerNetworkIndex.playerType.Sab)
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
            if (!jump.IsPressed() && !crouch.IsPressed())
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, slowRate);
            }
        }
        else
        {
            rb.useGravity = true;
        }
        if(jump.WasPerformedThisFrame())
        {
            OnJump();
        }

    }

    public void OnMove()
    {
        var v = move.ReadValue<Vector2>();
        if (PLI.PlayerType == PlayerNetworkIndex.playerType.Runner)
        {
            anim.SetFloat("Forward", v.y);
            anim.SetFloat("Strafe", v.x);
        }
        if (Dizzy)
        {
            v.y = Random.Range(-1, 1);
        }
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
        Vector3 moveVelocity = moveDirection * moveSpeed;
        if (CanFly)
        {
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        else
        {
            rb.AddForce(moveDirection * moveSpeed);
            Vector2 normalized = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
            normalized = Vector2.ClampMagnitude(normalized, maxVelocity);
            rb.linearVelocity = new Vector3(normalized.x, rb.linearVelocity.y, normalized.y);
        }
    }
}
