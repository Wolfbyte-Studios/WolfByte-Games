using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 Gravity = new Vector3(0, -9.81f, 0);
    public float MovementSpeed;
    public float MovementAcceleration = 0.02f;
    public float RotationSpeed;
    public float jumpHeight;

    private Vector3 velocity;
    private bool isGrounded;

    private CharacterController cc;
    private Animator anim;
    private GameObject Player;
    private Camera playerCam;
    private InputAction inputMove;
    private InputAction inputJump;
    private InputAction inputAttack;
    private InputAction inputInteract;
    private InputAction inputItem;
    private InputAction inputRun;
    private bool isRunning = false;

    void Start()
    {
        anim = transform.GetComponentInChildren<Animator>();
        cc = transform.GetComponentInChildren<CharacterController>();

        inputMove = InputSystem.actions.FindAction("Move");
        inputAttack = InputSystem.actions.FindAction("Attack");
        inputAttack.performed += OnAttack;
        inputJump = InputSystem.actions.FindAction("Jump");
        inputJump.performed += OnJump;
        inputInteract = InputSystem.actions.FindAction("Interact");
        inputInteract.performed += OnInteract;
        inputItem = InputSystem.actions.FindAction("Item");
        inputItem.performed += OnItem;
        inputRun = InputSystem.actions.FindAction("Run");
        inputRun.performed += OnRun;
        Player = cc.transform.gameObject;
        playerCam = transform.GetComponentInChildren<Camera>();
    }

    public void ProcessMovement()
    {
       
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        if (!inputMove.IsPressed())
        {
            if (MovementSpeed > 1f)
            {
                MovementSpeed = Mathf.Lerp(MovementSpeed, 0f, MovementAcceleration);
            }
            else
            {
                MovementSpeed = Mathf.Lerp(MovementSpeed, 0f, MovementAcceleration * 3);
            }

            return;
        }

        var value = inputMove.ReadValue<Vector2>();
        var lookDirection = playerCam.transform.right * value.x + playerCam.transform.forward * value.y;
        lookDirection.y = 0; // Ensure the movement is only on the XZ plane

        if (lookDirection.sqrMagnitude > 0.01f) // Check to avoid small rotations when not moving
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection * -1);
            Player.transform.rotation = Quaternion.Slerp(Player.transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        }
        if (isRunning)
        {
            // Running
            MovementSpeed = Mathf.Lerp(MovementSpeed, 10f, MovementAcceleration);
        }
        else
        {
            // Walking
            MovementSpeed = Mathf.Lerp(MovementSpeed, 1f, MovementAcceleration * 2);
        }

        

        
        
        
    }

    public void OnInteract(InputAction.CallbackContext obj)
    {
        // Player interaction logic here
    }
    public void OnRun(InputAction.CallbackContext obj)
    {
        isRunning = !isRunning;   
    }
    public void OnJump(InputAction.CallbackContext obj)
    {
        
            anim.SetTrigger("Jump");
        
    }
    
    public void Jump()
    {
        
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Gravity.y);
            cc.Move(velocity * Time.deltaTime);
            isGrounded = false;
            Debug.Log("Jumped with physics");
    }

    public void OnAttack(InputAction.CallbackContext obj)
    {
        // Player attack logic here
    }

    public void OnItem(InputAction.CallbackContext obj)
    {
        // Player item use logic here
    }


    public void FixedUpdate()
    {
        isGrounded = cc.isGrounded;
        anim.SetBool("IsGrounded", cc.isGrounded);
        ProcessMovement();
        // Apply gravity
        velocity.y += Gravity.y * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
        //Apply animator components
        anim.SetFloat("MovementSpeed", MovementSpeed);


    }
}
