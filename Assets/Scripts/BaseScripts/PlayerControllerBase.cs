using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using CodeMonkey; // Add Netcode namespace

public class PlayerControllerBase : NetworkBehaviour
{
    [Header("Movement")]
    [HideInInspector]
    public float speed;
    public float standingSpeed = 10f;
    public float jumpHeight = 12f;
    public float groundDrag = 5f;
    public float airDrag = 1f;
    public float airSpeedMultiplier = 0.4f;

    [HideInInspector]
    public Vector3 moveDirection;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    private LayerMask ground;
    [HideInInspector]
    public bool isGrounded;

    [Header("Crouching")]
    public float crouchingSpeed = 5f;
    public float crouchYScale = 0.5f;
    private float standYScale;

    [Header("Sliding")]
    public float maxSlideTime = 0.5f;
    public float slideSpeed = 15f;
    private float slideTimer;

    [Header("WallRunning")]
    private WallRunningBase wallRunning;

    [Header("Climbing")]
    private Climbing climbing;

    [Header("SlopeHandling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;
    private Transform orientation;

    [Header("Miscellaneous")]
    public List<Material> teamColorMaterials;
    public GameObject spawnPoint;
    public NetworkVariable<FixedString32Bytes> playerTeam = new NetworkVariable<FixedString32Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool playerMatIsSet;
    public SkinnedMeshRenderer meshRenderer;

    private InputActions inputActions;
    private InputAction movement;
    [HideInInspector]
    public Rigidbody rb;
    private PlayerSounds audioPlayer;
    public float animXVal;
    public float animYVal;

    public MovementState state;

    public Animator animator;

    public enum MovementState
    {
        standing,
        crouching,
        sliding,
        wallRunning,
        climbing,
        falling
    };

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb.freezeRotation = true;
        inputActions = new InputActions();
        LoadRebinds(inputActions.asset);
        movement = inputActions.Player.Movement;
        standYScale = transform.localScale.y;
        speed = standingSpeed;
        state = MovementState.standing;
        slideTimer = maxSlideTime;
        wallRunning = GetComponent<WallRunningBase>();
        climbing = GetComponent<Climbing>();
        audioPlayer = GameObject.Find("AudioManager").GetComponent<PlayerSounds>();
        orientation = transform.Find("Orientation");
        ground = LayerMask.GetMask("ground", "Stage", "wall");
    }

    public void LoadRebinds(InputActionAsset inputActionAsset)
    {
        if (PlayerPrefs.HasKey("Rebinds"))
        {
            Debug.Log("Loading Rebinds");
            string rebinds = PlayerPrefs.GetString("Rebinds");
            inputActionAsset.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("Loaded Rebinds: " + rebinds);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (isOwner())
        {
            StartCoroutine(InitializeTeamWithDelay());
        }

        playerTeam.OnValueChanged += OnTeamChanged;
    }

    private IEnumerator InitializeTeamWithDelay()
    {
        // Small delay to ensure the connection is established
        yield return new WaitForSeconds(0.5f);

        string team = PlayerPrefs.GetString("Team");
        if (!string.IsNullOrEmpty(team))
        {
            SetTeamOnServerRpc(team);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerTeam.OnValueChanged -= OnTeamChanged;
    }

    [ServerRpc]
    private void SetTeamOnServerRpc(string team)
    {
        playerTeam.Value = new FixedString32Bytes(team);
    }

    private void OnTeamChanged(FixedString32Bytes oldTeam, FixedString32Bytes newTeam)
    {
        // Update the material whenever the team changes
        UpdateMaterial(newTeam.ToString());
    }

    private void UpdateMaterial(string team)
    {
        if (team == "Blue")
        {
            meshRenderer.material = teamColorMaterials[0];
            playerMatIsSet = true;
        }
        else if (team == "Red")
        {
            meshRenderer.material = teamColorMaterials[1];
            playerMatIsSet = true;
        }
    }

    public virtual void Start()
    {
        if (isNotOwner()) return;

        respawnPlayer();
    }

    public virtual void Update()
    {
        //This is a very brute force way of doing this but we don't have time for optimization right now
        if(!playerMatIsSet)
        {
            UpdateMaterial(playerTeam.Value.ToString());
        }

        if (isNotOwner()) return;

        if (rb.isKinematic)
            rb.isKinematic = false;

        checkGrounded();
        limitSpeed();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public virtual void FixedUpdate()
    {
        if (isNotOwner()) return;
        animator.SetInteger("MovementState", (int)state);
        animator.SetFloat("MoveX", animXVal);
        animator.SetFloat("MoveY", animYVal);
        Debug.Log("MovementState: " +  state); 
        switch (state)
        {
            case MovementState.standing:
                speed = standingSpeed;
                if (transform.localScale.y != standYScale) 
                    transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
                movePlayer();
                break;
            case MovementState.crouching:
                speed = crouchingSpeed;
                movePlayer();
                break;
            case MovementState.falling:
                movePlayer();
                rb.AddForce(Vector3.down * 15f, ForceMode.Force);
                break;
            case MovementState.sliding:
                speed = slideSpeed;
                slideMovement();
                break;
            default:
                Vector2 v2 = movement.ReadValue<Vector2>();
                animXVal = v2.x;
                animYVal = v2.y;
                break;
        }
    }

    private void OnEnable()
    {
        movement.Enable();
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.Jump.Enable();
        inputActions.Player.Crouch.performed += Crouch;
        inputActions.Player.Crouch.canceled += Uncrouch;
        inputActions.Player.Crouch.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        inputActions.Player.Jump.performed -= Jump;
        inputActions.Player.Jump.Disable();
        inputActions.Player.Crouch.performed -= Crouch;
        inputActions.Player.Crouch.canceled -= Uncrouch;
        inputActions.Player.Crouch.Disable();
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        
        if (isGrounded)
        {
            animator.SetTrigger("Jump");
            //if (state == MovementState.sliding)
            //    transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            audioPlayer.playSound("Jump");
        }
        else if (state == MovementState.wallRunning)
        {
            animator.SetTrigger("Jump");
            wallRunning.wallJump();
            audioPlayer.playSound("Jump");
        }
    }

    private void Crouch(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            //transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            if ((moveDirection.x != 0 || moveDirection.z != 0) && state != MovementState.sliding)
            {
                if (!onSlope() || rb.velocity.y <= -0.1f)
                    startSlide();
            }
            else
                state = MovementState.crouching;
        }
    }

    private void Uncrouch(InputAction.CallbackContext obj)
    {
        if (isNotOwner()) return;

        if (state != MovementState.sliding)
        {
            //transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            state = MovementState.standing;
        }
    }

    private void startSlide()
    {
        if (isNotOwner()) return;

        audioPlayer.playSound("Slide");
        state = MovementState.sliding;
        slideTimer = maxSlideTime;
        Vector2 v2 = movement.ReadValue<Vector2>();
        animXVal = v2.x;
        animYVal = v2.y;
        //Debug.Log(animYVal);
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;
    }

    private void respawnPlayer()
    {
        string spawnTeam = PlayerPrefs.GetString("Team");

        if (spawnPoint == null)
        {
            if (spawnTeam == "Red")
            {
                spawnPoint = GameObject.Find("RedTeamSpawn") ?? GameObject.Find("TeamSpawn");
            }
            else if (spawnTeam == "Blue")
            {
                spawnPoint = GameObject.Find("BlueTeamSpawn") ?? GameObject.Find("TeamSpawn");
            }
            else
            {
                Debug.Log("Failed to properly spawn player.");
                spawnPoint = GameObject.Find("DefaultSpawn");
            }
        }

        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;
    }

    public void Die()
    {
        if (isNotOwner()) return;

        audioPlayer.playSound("Die");
        respawnPlayer();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isNotOwner()) return;

        if (other.gameObject.CompareTag("DeathPlane"))
        {
            Die();
        }
    }

    private void slideMovement()
    {
        if (isNotOwner()) return;
        
        if (!onSlope())
        {
            rb.AddForce(moveDirection.normalized * slideSpeed * 10f, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else if (rb.velocity.y <= -0.1f)
        {
            rb.AddForce(getSlopeMovementDirection(moveDirection) * slideSpeed * 10f, ForceMode.Force);
        }

        if (slideTimer <= 0f)
            stopSlide();
    }

    private void stopSlide()
    {
        if (isNotOwner()) return;

        audioPlayer.stopSound("Slide");
        //transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
        state = isGrounded ? MovementState.standing : MovementState.falling;
    }

    private void movePlayer()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        animXVal = v2.x;
        animYVal = v2.y;
        
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;

        Debug.Log(rb);

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * speed * 10f * airSpeedMultiplier, ForceMode.Force);
    
    }

    private void limitSpeed()
    {
        if (isNotOwner()) return;

        if (onSlope())
        {
            if (rb.velocity.magnitude > speed)
                rb.velocity = rb.velocity.normalized * speed;
        }
        else
        {
            Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (velocity.magnitude > speed)
            {
                Vector3 limitedVelocity = velocity.normalized * speed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void checkGrounded()
    {
        if (isNotOwner()) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.6f, ground);

        if (isGrounded)
        {
            rb.drag = groundDrag;

            // Transition back to standing state if grounded
            if (state == MovementState.falling)
            {
                state = MovementState.standing;
            }
        }
        else
        {
            rb.drag = airDrag;

            // Ensure player transitions to falling if not grounded
            if (state != MovementState.sliding && state != MovementState.wallRunning && state != MovementState.climbing)
            {
                state = MovementState.falling;
            }
        }
    }

    private bool onSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 getSlopeMovementDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private bool isNotOwner()
    {
        return PlayerPrefs.GetString("Mode") == "Online" && !IsOwner;
    }

    private bool isOwner()
    {
        return PlayerPrefs.GetString("Mode") == "Online" && IsOwner;
    }
}
