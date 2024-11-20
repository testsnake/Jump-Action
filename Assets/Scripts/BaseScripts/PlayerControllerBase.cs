using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode; // Add Netcode namespace

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
    public string playerTeam;
    public bool colorIsSet;

    private InputActions inputActions;
    private InputAction movement;
    [HideInInspector]
    public Rigidbody rb;
    private PlayerSounds audioPlayer;

    public MovementState state;

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
        rb.freezeRotation = true;
        inputActions = new InputActions();
        movement = inputActions.Player.Movement;
        standYScale = transform.localScale.y;
        speed = standingSpeed;
        state = MovementState.standing;
        slideTimer = maxSlideTime;
        wallRunning = GetComponent<WallRunningBase>();
        climbing = GetComponent<Climbing>();
        audioPlayer = GameObject.Find("AudioManager").GetComponent<PlayerSounds>();
        orientation = transform.Find("Orientation");
        ground = LayerMask.GetMask("ground", "Stage");
        colorIsSet = false;
    }

    private void attemptSetColor()
    {
        if (!string.IsNullOrEmpty(playerTeam))
        {
            MeshRenderer meshRenderer = transform.Find("PlayerBody").gameObject.GetComponent<MeshRenderer>();
            //If, when we implement the animated model for the player, they have multiple materials, then one of two things needs to happen here
            //Either we make sure that the material we want to swap is the first material in the list in the renderer
            //Or we set the list of materials in here and change the entire list (because we unfortunately can't just change one element for some reason.
            if (playerTeam == "Blue")
            {
                meshRenderer.material = teamColorMaterials[0];
                colorIsSet = true;
            }
            else if (playerTeam == "Red")
            {
                meshRenderer.material = teamColorMaterials[1];
                colorIsSet = true;
            }
        }
    }

    public virtual void Start()
    {
        Debug.Log("IsOwner for " + gameObject.name + ": " + IsOwner);
        if (IsOwner)
            playerTeam = PlayerPrefs.GetString("Team");
        attemptSetColor();
        if (IsOwner)
            respawnPlayer();
    }

    public virtual void Update()
    {
        if (!IsOwner) return;

        checkGrounded();
        limitSpeed();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (!colorIsSet)
        {
            attemptSetColor();
        }
    }

    public virtual void FixedUpdate()
    {
        if (!IsOwner) return;

        switch (state)
        {
            case MovementState.standing:
                speed = standingSpeed;
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
            case MovementState.wallRunning:
                // WallRunning Script Does Everything
                break;
            case MovementState.climbing:
                // Climbing Script Does Everything
                break;
            default:
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
            if (state == MovementState.sliding)
                transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            audioPlayer.playSound("Jump");
        }
        else if (state == MovementState.wallRunning)
        {
            wallRunning.wallJump();
            audioPlayer.playSound("Jump");
        }
    }

    private void Crouch(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
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
        if (!IsOwner) return;

        if (state != MovementState.sliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            state = MovementState.standing;
        }
    }

    private void startSlide()
    {
        if (!IsOwner) return;

        audioPlayer.playSound("Slide");
        state = MovementState.sliding;
        slideTimer = maxSlideTime;
        Vector2 v2 = movement.ReadValue<Vector2>();
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;
    }

    private void respawnPlayer()
    {

        if (spawnPoint == null)
        {
            if (playerTeam == "Red")
            {
                spawnPoint = GameObject.Find("RedTeamSpawn");
            }
            else if (playerTeam == "Blue")
            {
                spawnPoint = GameObject.Find("BlueTeamSpawn");
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
        if (!IsOwner) return;

        audioPlayer.playSound("Die");
        transform.position = new Vector3(0, 2, 0);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.CompareTag("DeathPlane"))
        {
            Die();
        }
    }

    private void slideMovement()
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;

        audioPlayer.stopSound("Slide");
        transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
        state = isGrounded ? MovementState.standing : MovementState.falling;
    }

    private void movePlayer()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * speed * 10f * airSpeedMultiplier, ForceMode.Force);
    }

    private void limitSpeed()
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.4f, ground);

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
}
