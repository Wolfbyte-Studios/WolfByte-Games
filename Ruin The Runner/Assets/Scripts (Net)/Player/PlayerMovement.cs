using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;
using JetBrains.Annotations;
using System;

public class PlayerMovement : NetworkBehaviour
{
    
    public enum playertype
    {
        Sab,
        Runner
    }
    [Header("Player Type")]
    public playertype PlayerType;
    [Header("Movement Settings")]
    public float moveSpeed;
    public float maxVelocity;
    public bool CanFly;
    public float FlyForce = 50;
    public float jumpHeight;
    public float slowRate;
    [SyncVar]
    public Vector3 velocity;

    public bool Dizzy = false;

    public GameObject playerCam;
    public GameObject Mallet = null;
    public Rigidbody rb;
    public InputActionAsset actions;
    public InputAction move;
    public InputAction jump;
    public InputAction crouch;
    public InputAction menu;
    public InputAction Fire;
    public InputAction Secondary;
    public Animator anim;
    public NetworkAnimator NAnim;
    public GameObject Shit;
    public Vector3 shitOffset;
    //private PlayerNetworkIndex PLI;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Setup();
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);

    }
    public void OnEnable()
    {
        Setup();
    }
    public void Start()
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
        if(!isLocalPlayer) { return; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = gameObject.GetComponent<Rigidbody>();
        NAnim = GetComponent<NetworkAnimator>();

        var playerInput = gameObject.transform.parent.GetComponent<PlayerInput>();
        //PLI = gameObject.GetComponent<PlayerNetworkIndex>();

        if (playerInput != null)
        {
            Debug.Log("Setup part 1 success");
            actions = playerInput.actions; // Get the actions from the PlayerInput component
            move = actions.FindAction("Move");
            menu = actions.FindAction("Menu");
            menu.performed += Menu_performed;
            jump = actions.FindAction("Jump");
            crouch = actions.FindAction("Crouch");
            crouch.performed += OnCrouch;

        }
        playerCam = transform.parent.Find("PlayerCam").gameObject;
        
        if (PlayerType == playertype.Sab)
        {
            playerCam.gameObject.GetComponent<Camera>().cullingMask = -1;
            // Get the current culling mask of the camera
            int currentMask = playerCam.gameObject.GetComponent<Camera>().cullingMask;

            // Calculate the new culling mask to exclude layer 11
            int layer11Mask = 1 << 11;
            int newMask = currentMask & ~layer11Mask;

            // Set the new culling mask to the camera
            playerCam.gameObject.GetComponent<Camera>().cullingMask = newMask;
            playerCam.gameObject.GetComponent<CinemachineFollow>().FollowOffset = Vector3.zero;
            CanFly = true;
        }
        else if (PlayerType == playertype.Runner)
        {
            playerCam.gameObject.GetComponent<Camera>().cullingMask = -1;
            Debug.Log("Setup part 2 success");
            Mallet = this.gameObject.transform.FindDeepChild("mallet").gameObject;
            Mallet.SetActive(false);
            playerCam.gameObject.GetComponent<CinemachineFollow>().FollowOffset = new Vector3(0, 1.75f, 0);
            CanFly = false;
            Fire = actions.FindAction("Fire");
            Fire.performed += Fire_performed;
            Secondary = actions.FindAction("Secondary");
            Secondary.performed += Secondary_performed;
        }
        playerCam.gameObject.GetComponent<CinemachineCamera>().Follow = this.gameObject.transform;
        

    }

    private void Secondary_performed(InputAction.CallbackContext obj)
    {
        anim.SetTrigger("Secondary");
        NAnim.SetTrigger("Secondary");
        NetworkUtils.RpcHandler(this, onPoop);
        //throw new System.NotImplementedException();
    }

    private void Fire_performed(InputAction.CallbackContext obj)
    {
        anim.SetTrigger("Primary");
        NAnim.SetTrigger("Primary");
        if (PlayerType == playertype.Sab)
        {
            return;
        }
        StartCoroutine(mallet());
        //throw new System.NotImplementedException();
    }
    IEnumerator mallet()
    {
        NetworkUtils.RpcHandler(this, setMallet);
        yield return new WaitForSeconds(1f);
        NetworkUtils.RpcHandler(this, setMallet);
        yield return null;
    }
    public void setMallet()
    {
        Mallet.SetActive(!Mallet.activeSelf);
    }
    private void Menu_performed(InputAction.CallbackContext obj)
    {
        var cursorVis = !Cursor.visible;
        CursorFree(cursorVis);
        Debug.Log("The cursor was " + Cursor.visible + " and now it is " + cursorVis);

    }
    public void CursorFree(bool tru)
    {
        if (tru)
        {

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
    }
    [Command]
    public void onPoop()
    {
        Vector3 offset = (shitOffset.x * this.gameObject.transform.right) +
                  (shitOffset.y * this.gameObject.transform.up) +
                  (shitOffset.z * this.gameObject.transform.forward);

        var poop = Instantiate(Shit, this.gameObject.transform.position + offset, Quaternion.identity);
        NetworkServer.Spawn(poop, netIdentity.gameObject);
    }
    public void OnCrouch(InputAction.CallbackContext obj)
    {
        if (CanFly)
        {
            OnFly(-FlyForce);
            ////Debug.Log("Go down");
            return;
        }

        //Add crouch animation
        ////throw new System.NotImplementedException();
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
    public void centerModel()
    {
        gameObject.transform.GetChild(0).transform.localEulerAngles = Vector3.zero;
        gameObject.transform.GetChild(0).transform.localPosition = Vector3.zero;
    }
    public void FaceCamera()
    {
        
        if (PlayerType == playertype.Sab)
        {
            gameObject.transform.eulerAngles = new Vector3(playerCam.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
            return;
        }
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
        

    }
    
    public void FixedUpdate()
    {
        centerModel();
        if (!isLocalPlayer)
        {
           
            return;
        }
        FaceCamera();
        Debug.Log(this.gameObject.name + " is the one running this script!");
        if (move.IsPressed())
        {
            Debug.Log("Moving success");
            NetworkUtils.RpcHandler(this,  OnMove);
        }
        if (CanFly)
        {
            rb.useGravity = false;
            if (!jump.IsPressed() && !crouch.IsPressed())
            {
                NetworkUtils.RpcHandler(this, slowVelocity);
            }
        }
        else
        {
            rb.useGravity = true;
        }
        if (jump.WasPerformedThisFrame())
        {
            Debug.Log("Jump success");
            OnJump();
        }

    }
    public void Respawn()
    {
        this.gameObject.transform.position = GameManager.singleton.lastPooped.position;
        Debug.Log("respawned");
    }
    public void slowVelocity()
    {
        velocity = rb.linearVelocity;
        velocity = Vector3.Lerp(velocity, Vector3.zero, slowRate);
        rb.linearVelocity = velocity;
    }
    public void OnMove()
    {
        Debug.Log("Moving");
        var v = move.ReadValue<Vector2>();
        if (PlayerType == playertype.Runner)
        {
            anim.SetFloat("Forward", v.y);
            anim.SetFloat("Strafe", v.x);
        }
        if (Dizzy)
        {
            v.y = UnityEngine.Random.Range(-1, 1);
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
