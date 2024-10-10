using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Movement")]
    public float wallRunningSpeed = 12f;
    public float wallRunForce = 200f;
    public float wallJumpUpForce = 10f;
    public float wallJumpSideForce = 5f;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Wall Detection")]
    public LayerMask wall;
    public LayerMask ground;
    public float wallCheckDistance;
    public float minHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Gravity")]
    private bool useGravity = true;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private PlayerController player;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        checkForWall();
        if ((wallRight || wallLeft) && player.moveDirection.z != 0 && AboveGround())
        {
            if (player.state != PlayerController.MovementState.wallRunning)
                startWallRun();
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0 && player.state == PlayerController.MovementState.wallRunning)
                endWallRun();
        }
        else
        {
            if (player.state == PlayerController.MovementState.wallRunning)
                endWallRun();
        }
    }

    void FixedUpdate()
    {
        if (player.state == PlayerController.MovementState.wallRunning)
            wallRunningMovement();
    }

    void checkForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minHeight, ground);
    }

    void startWallRun()
    {
        player.state = PlayerController.MovementState.wallRunning;
        player.speed = wallRunningSpeed;
        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        cam.DoFov(90f);
        if (wallLeft)
            cam.DoTilt(-5f);
        else if (wallRight)
            cam.DoTilt(5f);
    }

    void wallRunningMovement()
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

    void endWallRun()
    {
        player.state = PlayerController.MovementState.standing;
        player.speed = player.standingSpeed;

        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    public void wallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallJumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(wallJumpForce, ForceMode.Impulse);
    }

}
