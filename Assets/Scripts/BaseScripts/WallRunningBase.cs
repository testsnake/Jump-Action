using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningBase : MonoBehaviour
{
    [Header("Movement")]
    public float wallRunningSpeed = 12f;
    public float wallRunForce = 200f;
    public float wallJumpUpForce = 10f;
    public float wallJumpSideForce = 5f;
    public float maxWallRunTime = 1.5f;
    protected float wallRunTimer;

    [Header("Wall Detection")]
    public LayerMask wall;
    public LayerMask ground;
    public float wallCheckDistance = 0.7f;
    public float minHeight = 1f;
    protected RaycastHit leftWallHit;
    protected RaycastHit rightWallHit;
    protected bool wallLeft;
    protected bool wallRight;
    protected Transform lastWall;

    [Header("Gravity")]
    protected bool useGravity = true;
    public float gravityCounterForce = 5f;

    [Header("References")]
    public Transform orientation;
    private PlayerCamBase cam;
    protected PlayerControllerBase player; // Use PlayerControllerBase for modularity
    protected Rigidbody rb;

    public virtual void Start()
    {
        // Setup references
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerControllerBase>();
        cam = GameObject.Find("CameraHolder")?.GetComponent<PlayerCamBase>();
        orientation = transform.Find("Orientation");
    }
    protected virtual void startWallRun()
    {
        player.state = PlayerControllerBase.MovementState.wallRunning;
        player.speed = wallRunningSpeed;
        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        lastWall = wallRight ? rightWallHit.transform : leftWallHit.transform;

        cam.DoFov(90f);
        cam.DoTilt(wallLeft ? -15f : 15f);
    }

    protected virtual void endWallRun()
    {
        player.state = PlayerControllerBase.MovementState.standing;
        player.speed = player.standingSpeed;
        
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }


    public virtual void Update()
    {
        // Wall detection and wall-running logic
        checkForWall();
        if (AboveGround() && (wallRight || wallLeft) && player.moveDirection.z != 0)
        {
            if (player.state != PlayerControllerBase.MovementState.wallRunning)
                startWallRun();

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && player.state == PlayerControllerBase.MovementState.wallRunning)
                endWallRun();
        }
        else
        {
            if (player.state == PlayerControllerBase.MovementState.wallRunning)
                endWallRun();
        }
    }

    public virtual void FixedUpdate()
    {
        if (player?.state == PlayerControllerBase.MovementState.wallRunning)
            wallRunningMovement();
    }

    protected virtual void checkForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wall);
    }


    protected virtual bool AboveGround()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, minHeight, ground);

        if (isGrounded)
        {
            cam?.DoTilt(0f);
            lastWall = null;
        }

        return !isGrounded;
    }

    protected virtual void wallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward + wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (useGravity)
            rb.AddForce(Vector3.up * gravityCounterForce, ForceMode.Force);
    }

    public virtual void wallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallJumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(wallJumpForce, ForceMode.Impulse);
    }
}
