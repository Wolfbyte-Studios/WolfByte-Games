using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;
using JetBrains.Annotations;
using System;
using System.Linq;

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
    public InputAction debugSwitch;
    public InputAction respawn;
    public Animator anim;
    public NetworkAnimator NAnim;
    public GameObject Shit;
    public Vector3 shitOffset;
    private Collision lastCollided;
    public Transform followTarget;
    public LayerMask excludedLayers;
    public Vector3 GroundcheckOrigin;
    public float GroundcheckLength;
    [SerializeField]
    private float angle;
    public float stairSpeed;
    private PlayerIdentity PLI;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Setup();
        //DontDestroyOnLoad(gameObject.transform.parent.gameObject);

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
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        debugSwitch.performed -= DebugSwitch_performed;
        respawn.performed -= Respawn_performed;

        menu.performed -= Menu_performed;
        crouch.performed -= OnCrouch;
        Fire.performed -= Fire_performed;
        Secondary.performed -= Secondary_performed;
    }

    public void Setup()
    {
        if (!isLocalPlayer) { return; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = gameObject.GetComponent<Rigidbody>();
        NAnim = GetComponent<NetworkAnimator>();
        debugSwitch = actions.FindAction("debugSwitch");
        debugSwitch.performed += DebugSwitch_performed;
        respawn = actions.FindAction("Respawn");
        respawn.performed += Respawn_performed;
        var playerInput = gameObject.transform.parent.GetComponent<PlayerInput>();
        PLI = gameObject.transform.parent.GetComponent<PlayerIdentity>();

        if (playerInput != null)
        {
            //Debug.log("Setup part 1 success");
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
            //Debug.log("Setup part 2 success");
            Mallet = this.gameObject.transform.FindDeepChild("mallet").gameObject;
            Mallet.SetActive(false);
            //playerCam.gameObject.GetComponent<CinemachineFollow>().FollowOffset = new Vector3(0, 1.75f, 0);
            CanFly = false;
            Fire = actions.FindAction("Fire");
            Fire.performed += Fire_performed;
            Secondary = actions.FindAction("Secondary");
            Secondary.performed += Secondary_performed;
        }
        playerCam.gameObject.GetComponent<CinemachineCamera>().Follow = this.followTarget;

        Respawn();
    }

    private void Respawn_performed(InputAction.CallbackContext obj)
    {
        Respawn();
        throw new NotImplementedException();
    }

    private void DebugSwitch_performed(InputAction.CallbackContext obj)
    {
        GameManager.singleton.RotatePlayersDebug();
        //throw new NotImplementedException();
    }

    private void Secondary_performed(InputAction.CallbackContext obj)
    {
        anim.SetTrigger("Secondary");
        NAnim.SetTrigger("Secondary");
        if (PlayerType == playertype.Runner)
        {
            NetworkUtils.RpcHandler(this, onPoop);
            //throw new System.NotImplementedException();
        }
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
        StartCoroutine(resetTriggers());
        //throw new System.NotImplementedException();
    }
    IEnumerator resetTriggers()
    {
        yield return new WaitForSeconds(.15f);
        anim.ResetTrigger("Primary");
        NAnim.ResetTrigger("Primary");
        yield return null;
    }
    IEnumerator mallet()
    {
        NetworkUtils.RpcHandler(this, setMallet);
        yield return new WaitForSeconds(1.93f);
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
        //Debug.log("The cursor was " + Cursor.visible + " and now it is " + cursorVis);

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
            FlyForce = -Math.Abs(FlyForce);
            NetworkUtils.RpcHandler(this, OnFly);
            //////Debug.log("Go down");
            return;
        }

        //Add crouch animation
        ////throw new System.NotImplementedException();
    }

    public void OnJump()
    {
        if (CanFly)
        {
            FlyForce = Mathf.Abs(FlyForce);
            NetworkUtils.RpcHandler(this, OnFly);
            return;
        }
        if (anim.GetBool("Grounded"))
        {

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            if (PlayerType == playertype.Runner)
            {
                anim.SetTrigger("Jump");
                NAnim.SetTrigger("Jump");
            }
        }
       
    }

    public void OnFly()
    {

        velocity.y = FlyForce;
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
    public void OnCollisionEnter(Collision collision)
    {
        lastCollided = collision;
        NetworkUtils.RpcHandler(this, handleCollision);
    }
    public void handleCollision()
    {
        if (lastCollided.gameObject.GetComponent<HoldItem>() != null)
        {
            var holdable = lastCollided.gameObject.GetComponent<HoldItem>();
            if (holdable.velocity.magnitude > 0)
            {
                // Normalize the direction and scale by the magnitude and strength
                Vector3 direction = holdable.velocity.normalized;
                float magnitude = holdable.velocity.magnitude * holdable.Strength;

                velocity = direction * magnitude;
                rb.linearVelocity = direction * magnitude;
                Debug.LogWarning(direction * magnitude);
            }
        }
        else if (lastCollided.gameObject.name == "mallet")
        {
            var malletrb = lastCollided.gameObject.GetComponent<Rigidbody>();
            if (malletrb.linearVelocity.magnitude > 0)
            {
                // Normalize the direction and scale by the magnitude and factor
                Vector3 direction = malletrb.linearVelocity.normalized;
                float magnitude = malletrb.linearVelocity.magnitude * 50;

                velocity = direction * magnitude;
                rb.linearVelocity = direction * magnitude;
            }
        }
    }
    public Vector3 stairOffset;
    public void Stairs(Vector3 target)
    {
        float step = stairSpeed * Time.deltaTime; // Calculate movement step based on speed
        if (target.y - transform.position.y > 0.1f && target.y - transform.position.y < 0.4f && move.ReadValue<Vector2>().y > 0)
        {
            var newTarget = target + transform.TransformDirection(stairOffset);
            transform.position = Vector3.Lerp(transform.position, newTarget, step);

            // Draw a point at newTarget's position
            Debug.DrawLine(newTarget, newTarget + Vector3.up * 0.1f, Color.red, 2f); // Adjust the Vector3.up * 0.1f to control the size of the point

            Debug.Log(step);
        }

    }
    public void checkGround()
    {
        Vector3 origin = transform.TransformPoint(GroundcheckOrigin);
        // Perform a raycast with a LayerMask
        RaycastHit hit;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out hit, GroundcheckLength, excludedLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject && hit.collider.isTrigger == false)
            {
                Debug.Log(hit.collider.name + ": " + hit.collider.gameObject.name);
                // Calculate the angle between the hit normal and the up vector (90 degrees is a flat surface)
                angle = Vector3.Angle(hit.normal, Vector3.up);
                anim.SetBool("Grounded", true);
                if (Mathf.Abs(angle) <= 35 && hit.point.y - transform.position.y >= 0.1f)
                {
                    Stairs(hit.point);
                }
                // Log the angle as a warning
                // Debug.LogWarning("Surface angle: " + angle + " degrees");

                // If the raycast hits something, you can handle it here
                // Debug.Log("Ground detected at distance: " + hit.distance);
            }
        }
        else
        {
            anim.SetBool("Grounded", false);
            // If no hit is detected
            // Debug.Log("No ground detected");
        }

        // Draw the raycast line for visualization in the Scene view
        Debug.DrawRay(origin, Vector3.down * GroundcheckLength, Color.red);
    }

    public void FixedUpdate()
    {
        velocity = rb.linearVelocity;
        checkGround();
        centerModel();
        if (!isLocalPlayer)
        {

            return;
        }
        FaceCamera();
        //Debug.log(this.gameObject.name + " is the one running this script!");
        if (move.IsPressed())
        {
            //Debug.log("Moving success");
            NetworkUtils.RpcHandler(this, OnMove);
            var v = move.ReadValue<Vector2>();
            anim.SetFloat("Forward", v.y * velocity.magnitude);
            anim.SetFloat("Strafe", v.x);

        }
        else
        {
            anim.SetFloat("Forward", 0f);
            anim.SetFloat("Strafe", 0f);
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
            NetworkUtils.RpcHandler(this, slowVelocity);
        }
        if (jump.WasPerformedThisFrame())
        {
            //Debug.log("Jump success");
            OnJump();
        }
        rb.linearVelocity = velocity;
    }
    [ContextMenu("Respawn")]
    public void Respawn()
    {
        if (GameManager.singleton.lastPooped != null)
        {
            this.gameObject.transform.position = GameManager.singleton.lastPooped.position;
        }
        else
        {
            var pedestals = GameObject.FindObjectsByType(typeof(PlayerPedestal), sortMode: FindObjectsSortMode.None)
                                 .Cast<PlayerPedestal>()
                                 .ToArray();

            // Sort the pedestals by distance from the current object (this script's GameObject)
            var sortedPedestals = pedestals.OrderBy(pedestal => Vector3.Distance(transform.position, pedestal.transform.position)).ToArray();

            // Now you can iterate over the sortedPedestals
            foreach (var pedestal in sortedPedestals)
            {
                if (pedestal.AcceptedPlayers.Contains(PLI.playerId)
                    &&
                    pedestal.AcceptedPlayertypes.Contains(this.PlayerType))
                {
                    this.gameObject.transform.position = pedestal.transform.position;

                    this.gameObject.transform.localEulerAngles = new Vector3(0, pedestal.transform.localEulerAngles.y, 0);
                    playerCam.GetComponent<CinemachinePanTilt>().PanAxis.Value = pedestal.transform.localEulerAngles.y;

                    return;
                }
                // Your logic here
            }
        }
        //Debug.log("respawned");
    }
    public void slowVelocity()
    {
        if (CanFly)
        {
            velocity = rb.linearVelocity;
            velocity = Vector3.Lerp(velocity, Vector3.zero, slowRate);
            rb.linearVelocity = velocity;
        }
        else
        {
            
            rb.linearVelocity = velocity;
            velocity = Vector3.Lerp(velocity, new Vector3( 0, velocity.y, 0), slowRate);
            rb.linearVelocity = velocity;
        }
    }
    public void OnMove()
    {
        //Debug.log("Moving");
        var v = move.ReadValue<Vector2>();
        if (PlayerType == playertype.Runner)
        {
            //anim.SetFloat("Forward", v.y);
            //anim.SetFloat("Strafe", v.x);
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
        velocity = rb.linearVelocity;
    }
}
