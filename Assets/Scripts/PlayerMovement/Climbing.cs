using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    public LayerMask wall;
    private PlayerController player;
    public PlayerCam cam;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    [Header("WallDetection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxAngle;
    private float wallAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;
    public float climbJumpRotationDuration;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        wallCheck();

        if (wallFront && (player.moveDirection.x * orientation.forward.x > 0.1f || player.moveDirection.z * orientation.forward.z > 0.1f) && wallAngle < maxAngle)
        {
            if (player.state == PlayerController.MovementState.falling && climbTimer > 0)
                startClimb();

            if (climbTimer > 0)
                climbTimer -= Time.deltaTime;
            else if (climbTimer <= 0)
                StopClimbing();
        }
        else if (player.state == PlayerController.MovementState.climbing)
            StopClimbing();

        if (player.state == PlayerController.MovementState.climbing)
            climbingMovement();
    }

    private void wallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wall);
        wallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    private void startClimb()
    {
        player.state = PlayerController.MovementState.climbing;
        player.speed = climbSpeed;
    }

    private void climbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        player.state = PlayerController.MovementState.standing;
        player.speed = player.standingSpeed;
    }

    public void climbJump()
    {
        Vector3 jumpForce = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(jumpForce, ForceMode.Impulse);
        cam.Rotate180(climbJumpRotationDuration);
    }
}
