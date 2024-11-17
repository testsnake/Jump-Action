using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ClimbingBase : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    protected Rigidbody rb;
    public LayerMask wall;
    protected PlayerControllerBase player; // Base player controller for modularity
    private PlayerCam cam;

    [Header("Climbing")]
    public float climbSpeed = 7f;
    public float maxClimbTime = 0.75f;
    protected float climbTimer;

    [Header("Wall Detection")]
    public float detectionLength = 0.7f;
    public float sphereCastRadius = 0.25f;
    public float maxAngle = 30f;
    protected float wallAngle;
    protected float tallWallAngle;
    protected RaycastHit frontWallHit;
    protected RaycastHit tallWallHit;
    protected bool wallFront;
    protected bool tallWallFront;

    [Header("Climb Jumping")]
    public float climbJumpUpForce = 14f;
    public float climbJumpBackForce = 12f;
    public float climbJumpRotationDuration = 0.3f;

    public virtual void Start()
    {
        // Setup references
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerControllerBase>(); // Reference to the base player controller
        orientation = transform.Find("Orientation");
        cam = GameObject.FindWithTag("MainCamera")?.GetComponent<PlayerCam>();

        // Safety checks
        if (orientation == null)
            Debug.LogError("Orientation is missing. Make sure your character has an Orientation child object.");
        if (player == null)
            Debug.LogError("PlayerControllerBase component is missing. Ensure the PlayerControllerBase is attached.");
        if (cam == null)
            Debug.LogWarning("PlayerCam is not assigned. Assign it in the Inspector if needed.");
    }

    public virtual void Update()
    {
        wallCheck();
        tallWallCheck();

        if (wallFront && playerIsHoldingForward() && wallAngle < maxAngle)
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
        return player.state == PlayerControllerBase.MovementState.falling && climbTimer > 0;
    }

    protected virtual bool canVault()
    {
        return !tallWallFront && climbTimer > 0;
    }

    protected virtual bool playerIsHoldingForward()
    {
        return player.moveDirection.x * orientation.forward.x > 0.1f || player.moveDirection.z * orientation.forward.z > 0.1f;
    }

    protected virtual void wallCheck()
    {
        Vector3 position = transform.position - new Vector3(0f, player.playerHeight * 0.25f, 0f);
        wallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wall);
        wallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    protected virtual void tallWallCheck()
    {
        Vector3 position = transform.position + new Vector3(0f, player.playerHeight * 0.125f, 0f);
        tallWallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wall);
        tallWallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    protected virtual void startClimb()
    {
        player.state = PlayerControllerBase.MovementState.climbing;
        player.speed = climbSpeed;
    }

    protected virtual void climbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    protected virtual void StopClimbing()
    {
        player.state = PlayerControllerBase.MovementState.standing;
        player.speed = player.standingSpeed;
    }

    public virtual void climbJump()
    {
        Vector3 jumpForce = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(jumpForce, ForceMode.Impulse);

        cam.Rotate180(climbJumpRotationDuration);

    }
}
