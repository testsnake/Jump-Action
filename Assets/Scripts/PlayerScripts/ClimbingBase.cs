using UnityEngine;
using Unity.Netcode;

public class ClimbingBase : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation; // Reference to player's Orientation
    protected Rigidbody rb; // Reference to player's rigidbody
    public LayerMask wall; // Reference to objects that can be run up
    public LayerMask smallWall; // Reference to objects that can be vaulted over 
    protected PlayerControllerBase player; // Base player controller for modularity
    private PlayerCamBase cam;

    [Header("Climbing")]
    public float climbSpeed = 7f;
    public float maxClimbTime = 0.75f;
    protected float climbTimer;

    [Header("Wall Detection")]
    public float detectionLength = 0.7f;
    public float sphereCastRadius = 0.25f;

    // Max angle at which the spherecast collision will be valid (Makes it so that player has to be somewhat facing the wall)
    public float maxAngle = 30f;
    protected float wallAngle;
    protected float tallWallAngle;
    protected RaycastHit frontWallHit;
    protected RaycastHit tallWallHit;
    protected bool wallFront;
    protected bool tallWallFront;

    public virtual void Start()
    {
        if (isNotOwner()) return;

        // Setup references
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerControllerBase>(); // Reference to the base player controller
        orientation = transform.Find("Orientation");
        cam = GameObject.Find("CameraHolder")?.GetComponent<PlayerCamBase>();

        // Safety checks
        if (orientation == null)
            Debug.LogError("Orientation is missing. Make sure your character has an Orientation child object.");
        if (player == null)
            Debug.LogError("PlayerControllerBase component is missing. Ensure the PlayerControllerBase is attached.");
        if (cam == null)
            Debug.LogWarning("PlayerCamBase is not assigned. Assign it in the Inspector if needed.");
    }

    public virtual void Update()
    {
        if (isNotOwner()) return;

        wallCheck();
        tallWallCheck();

        if ((wallFront || tallWallFront) && playerIsHoldingForward() && (wallAngle < maxAngle || tallWallAngle < maxAngle))
        {
            if (canClimb() || canVault())
                startClimb();

            if (climbTimer > 0)
                climbTimer -= Time.deltaTime;
            else if (climbTimer <= 0)
                StopClimbing();
        }
        else if (player.state == PlayerControllerBase.MovementState.climbing)
        {
            StopClimbing();
        }

        if (player.state == PlayerControllerBase.MovementState.climbing)
        {
            climbingMovement();
        }
    }

    protected virtual bool canClimb()
    {
        return tallWallFront && player.state == PlayerControllerBase.MovementState.falling && climbTimer > 0;
    }

    protected virtual bool canVault()
    {
        return wallFront && climbTimer > 0;
    }

    protected virtual bool playerIsHoldingForward()
    {
        return player.moveDirection.x * orientation.forward.x > 0f || player.moveDirection.z * orientation.forward.z > 0f;
    }

    protected virtual void wallCheck()
    {
        if (isNotOwner()) return;

        // Shoot raycast from a bit below the center of the player
        Vector3 position = transform.position - new Vector3(0f, player.playerHeight * 0.25f, 0f);
        wallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, smallWall);
        wallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    protected virtual void tallWallCheck()
    {
        if (isNotOwner()) return;

        // Shoot raycast from a bit above the center of the player
        Vector3 position = transform.position + new Vector3(0f, player.playerHeight * 0.125f, 0f);
        tallWallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wall);
        tallWallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    protected virtual void startClimb()
    {
        if (isNotOwner()) return;

        player.state = PlayerControllerBase.MovementState.climbing;
        player.speed = climbSpeed;
    }

    protected virtual void climbingMovement()
    {
        if (isNotOwner()) return;

        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    protected virtual void StopClimbing()
    {
        if (isNotOwner()) return;

        player.state = PlayerControllerBase.MovementState.standing;
        player.speed = player.standingSpeed;
    }

    private bool isNotOwner()
    {
        return PlayerPrefs.GetString("Mode") == "Online" && !IsOwner;
    }
}
