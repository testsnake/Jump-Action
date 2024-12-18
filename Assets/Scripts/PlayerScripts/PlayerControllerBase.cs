using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using UnityEngine.Animations.Rigging;
using System;

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
    private Transform playerCam;
    private PlayerCamBase camBase;
    private float cameraOffsetAim = 0.9f;

    [HideInInspector]
    public Vector3 moveDirection;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    private LayerMask ground;
    [HideInInspector]
    public bool isGrounded;

    [Header("Crouching")]
    public float crouchingSpeed = 5f;

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


    [Header("Rigging and Animation")]
    public RigBuilder rigBuilder;
    public MultiAimConstraint headAim;
    public MultiAimConstraint handAim;
    public TwoBoneIKConstraint armAim;
    private Transform lookTarget = null;
    private Transform aimTarget = null;
    public MovementState state;
    public Animator animator;
    private Animator hudGunAnimator;
    public CapsuleCollider collider;


    [Header("Extra")]
    [SerializeField] private GlobalVolumeManager globalVolumeManager;

    private enum CapsuleDirection
    {
        X = 0, // Capsule elongated along the X-axis
        Y = 1, // Capsule elongated along the Y-axis (default)
        Z = 2  // Capsule elongated along the Z-axis
    }

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
        speed = standingSpeed;
        state = MovementState.standing;
        slideTimer = maxSlideTime;
        wallRunning = GetComponent<WallRunningBase>();
        climbing = GetComponent<Climbing>();
        audioPlayer = GameObject.Find("AudioManager").GetComponent<PlayerSounds>();
        ground = LayerMask.GetMask("ground", "Stage", "wall");
        playerCam = GameObject.Find("CameraHolder").GetComponent<Transform>();
        hudGunAnimator = GameObject.Find("HUDGun").GetComponent<Animator>();
        camBase = playerCam.gameObject.GetComponent<PlayerCamBase>();
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
        else
        {
            StartCoroutine(InitializeTeamWithDelay());
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
        Health playerHealth = GetComponent<Health>();
        Material[] mats = new Material[3];
        if (team == "Blue")
        {
            mats[0] = teamColorMaterials[0];
            mats[1] = teamColorMaterials[1];
            mats[2] = teamColorMaterials[2];
            meshRenderer.materials = mats;
            playerMatIsSet = true;
            playerHealth.SetTeam("Blue");
        }
        else if (team == "Red")
        {
            mats[0] = teamColorMaterials[3];
            mats[1] = teamColorMaterials[4];
            mats[2] = teamColorMaterials[5];
            meshRenderer.materials = mats;
            playerMatIsSet = true;
            playerHealth.SetTeam("Red");
        }
    }

    private void SetClientLayerRecursive(GameObject gameObj)
    {
        gameObj.layer = LayerMask.NameToLayer("Client");
        foreach (Transform child in gameObj.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Client");

            Transform children = child.GetComponentInChildren<Transform>();
            if (children != null)
            {
                SetClientLayerRecursive(child.gameObject);
            }
        }
    }

    public virtual void Start()
    {
        if (isNotOwner()) return;
        SetClientLayerRecursive(gameObject);
        respawnPlayer();
        setRigTargets();
    }

    public void setRigTargets()
    {
        try
        {
            lookTarget = GameObject.Find("PlayerLookTarget").transform;
            aimTarget = GameObject.Find("PlayerAimTarget").transform;
            if (lookTarget != null)
            {
                animator.enabled = false;
                var sourceObj = headAim.data.sourceObjects;
                sourceObj.Clear();
                sourceObj.Add(new WeightedTransform(lookTarget, 1.0f));
                headAim.data.sourceObjects = sourceObj;

                sourceObj = handAim.data.sourceObjects;
                sourceObj.Clear();
                sourceObj.Add(new WeightedTransform(aimTarget, 1.0f));
                handAim.data.sourceObjects = sourceObj;

                armAim.data.target = aimTarget;
                rigBuilder.Build();
                animator.Rebind();
                animator.enabled = true;
            }
            else
            {
                Debug.LogWarning("Could not find target for player rig. Make sure there's an object named \"PlayerLookTarget\" as a child of the scene camera, for the player to look at. (Let me know if you need help! - Will)");
            }
        }
        catch
        {
            Debug.LogError("Error in setting the aim target during start. This may be bad.");
        }
    }

    public void UpdateMovementDirection()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        animXVal = v2.x;
        animYVal = v2.y;

        moveDirection = playerCam.forward * v2.y + playerCam.right * v2.x;
        if (isGrounded && moveDirection != Vector3.zero)
        {
            hudGunAnimator.SetBool("IsWalking", true);
        }
        else
        {
            hudGunAnimator.SetBool("IsWalking", false);
        }
        moveDirection.y = 0;
    }

    public virtual void Update()
    {
        //This is a very brute force way of doing this but we don't have time for optimization right now
        if (!playerMatIsSet)
        {
            UpdateMaterial(playerTeam.Value.ToString());
        }

        if (isNotOwner()) return;

        if (rb.isKinematic)
            rb.isKinematic = false;

        checkGrounded();
        limitSpeed();

        camBase.offset.y = Mathf.Lerp(camBase.offset.y, cameraOffsetAim, 0.3f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }


        if (globalVolumeManager != null)
        {
            globalVolumeManager.UpdateSpeed(rb.velocity);
        }
        else
        {
            GameObject gvm = GameObject.FindWithTag("GameManager");
            if (gvm != null)
            {
                globalVolumeManager = gvm.GetComponent<GlobalVolumeManager>();
            }
        }
    }

    public virtual void FixedUpdate()
    {
        if (isNotOwner()) return;
        animator.SetInteger("MovementState", (int)state);
        animator.SetFloat("MoveX", animXVal);
        animator.SetFloat("MoveY", animYVal);
        switch (state)
        {
            case MovementState.standing:
                hudGunAnimator.speed = 1f;
                speed = standingSpeed;
                movePlayer();
                break;
            case MovementState.crouching:
                hudGunAnimator.speed = 0.5f;
                speed = crouchingSpeed;
                movePlayer();
                break;
            case MovementState.falling:
                hudGunAnimator.speed = 1f;
                movePlayer();
                rb.AddForce(Vector3.down * 15f, ForceMode.Force);
                break;
            case MovementState.sliding:
                hudGunAnimator.speed = 1f;
                hudGunAnimator.SetBool("IsWalking", false);
                speed = slideSpeed;
                slideMovement();
                break;
            default:
                hudGunAnimator.speed = 1f;
                hudGunAnimator.SetBool("IsWalking", false);
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
            UpdateMovementDirection();
            if ((moveDirection.x != 0 || moveDirection.z != 0) && state != MovementState.sliding) // If is moving and is not currently sliding
            {
                if (!onSlope() || rb.velocity.y <= -0.1f) // If is not on slope or is going down a slope
                    startSlide();
            }
            else
                state = MovementState.crouching;

            shiftHitBox(); // Adjust player collider size and position according to Movement State
        }
    }

    private void Uncrouch(InputAction.CallbackContext obj)
    {
        if (isNotOwner()) return;

        if (state != MovementState.sliding)
        {
            state = MovementState.standing;
            shiftHitBox(); // Adjust player collider size and position according to Movement State
        }
    }

    private void startSlide()
    {
        if (isNotOwner()) return;

        audioPlayer.playSound("Slide");
        state = MovementState.sliding;
        slideTimer = maxSlideTime;
        UpdateMovementDirection();
    }

    private void respawnPlayer()
    {
        if (isNotOwner()) return;
        Debug.Log("Called respawnPlayer");

        string spawnTeam = PlayerPrefs.GetString("Team");

        if (PlayerPrefs.GetString("Mode") == "Offline")
        {
            spawnPoint = GameObject.Find("TeamSpawn");
            if (spawnPoint == null) throw Exception();
            rb.velocity = Vector3.zero;
            transform.position = spawnPoint.transform.position;
            Debug.Log("Spawning player at " + spawnPoint.transform.position);
        }
        else if (spawnTeam == "Red")
        {
            spawnPoint = GameObject.Find("RedTeamSpawn");
            if (spawnPoint == null) throw Exception();
            rb.velocity = Vector3.zero;
            transform.position = spawnPoint.transform.position;
            Debug.Log("Spawning player on red side at " + spawnPoint.transform.position);
        }
        else if (spawnTeam == "Blue")
        {
            spawnPoint = GameObject.Find("BlueTeamSpawn");
            if (spawnPoint == null) throw Exception();
            rb.velocity = Vector3.zero;
            transform.position = spawnPoint.transform.position;
            Debug.Log("Spawning player on blue side at " + spawnPoint.transform.position);
        }
        else
        {
            Debug.LogError("Failed to properly spawn player.");
            spawnPoint = null;
            rb.velocity = Vector3.zero;
            transform.position = GameObject.Find("DefaultSpawn").transform.position;
            respawnPlayer();
        }
    }

    private Exception Exception()
    {
        throw new NotImplementedException();
    }

    public void Die()
    {
        if (isNotOwner()) return;

        audioPlayer.playSound("Die");

        respawnPlayer();
        try
        {
            GameObject dataChipObject = gameObject.transform.Find("PlayerBody/DataChip").gameObject;

            if (dataChipObject == null) return;

            DataChip dataChip = dataChipObject.GetComponent<DataChip>();
            dataChip.ResetDataChip();
        }
        catch
        {

        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isNotOwner()) return;

        if (other.gameObject.CompareTag("DeathPlane"))
        {
            Die();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (isNotOwner()) return;
    }

    private void slideMovement()
    {
        if (isNotOwner()) return;
        UpdateMovementDirection();

        if (!onSlope())
        {
            rb.AddForce(moveDirection.normalized * slideSpeed * 10f, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else if (rb.velocity.y <= -0.1f) // If going down a slope
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
        state = isGrounded ? MovementState.standing : MovementState.falling;
        shiftHitBox(); // Adjust player collider size and position according to Movement State
    }

    private void movePlayer()
    {
        UpdateMovementDirection();

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
            // Adjust speed cap according to movement on slope inclination
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

    private void shiftHitBox()
    {
        Vector3 center = new Vector3(0, 1, 0);
        float radius = 0.5f;
        float height = 2f;
        CapsuleDirection direction = CapsuleDirection.Y;

        switch (state)
        {
            case MovementState.sliding:
                // Center: Vector3(-0.25, 0.35, 0.1)
                center = new Vector3(-0.25f, 0.75f, 0.1f);
                // Radius: 0.4
                radius = 0.4f;
                // Height: 2
                height = 2f;
                // Direction: Enum: Z-Axis
                direction = CapsuleDirection.Z;
                cameraOffsetAim = 0.5f;
                break;
            case MovementState.crouching:
                // Center: Vector3(0, 0.625, 0)
                center = new Vector3(0f, 0.625f, 0f);
                // Radius: 0.5
                radius = 0.5f;
                // Height: 1.25
                height = 1.25f;
                // Direction: Enum: Y-Axis
                direction = CapsuleDirection.Y;
                cameraOffsetAim = 0.5f;
                break;
            default: // Standing or any other state
                // Center: Vector3(0, 1, 0)
                center = new Vector3(0f, 1f, 0f);
                // Radius: 0.5
                radius = 0.5f;
                // Height: 2
                height = 2f;
                // Direction: Enum: Y-Axis
                direction = CapsuleDirection.Y;
                cameraOffsetAim = 0.9f;
                break;
        }

        collider.center = center;
        collider.radius = radius;
        collider.height = height;
        collider.direction = (int)direction;
    }
}
