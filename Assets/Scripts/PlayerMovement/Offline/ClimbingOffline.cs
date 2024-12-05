using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class ClimbingOffline : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    public LayerMask wall;
    public LayerMask smallWall;
    private PlayerControllerOffline player;
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
    private float tallWallAngle;
    private RaycastHit frontWallHit;
    private RaycastHit tallWallHit;
    private bool wallFront;
    private bool tallWallFront;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;
    public float climbJumpRotationDuration;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerControllerOffline>();
        orientation = transform.Find("Orientation");
    }

    // Update is called once per frame
    void Update()
    {
        wallCheck();
        tallWallCheck();

        Debug.Log(tallWallFront);

        if ((wallFront || tallWallFront) && playerIsHoldingForward() && (wallAngle < maxAngle || tallWallAngle < maxAngle)) // Player is running into wall or barrier
        {
            if (canClimb() || canVault())
                startClimb();

            if (climbTimer > 0)
                climbTimer -= Time.deltaTime;
            else if (climbTimer <= 0)
                StopClimbing();
        }
        else if (player.state == PlayerControllerOffline.MovementState.climbing)
            StopClimbing();

        if (player.state == PlayerControllerOffline.MovementState.climbing)
            climbingMovement();
    }

    private bool canClimb()
    {
        return tallWallFront && player.state == PlayerControllerOffline.MovementState.falling && climbTimer > 0;
    }

    private bool canVault()
    {
        return wallFront && climbTimer > 0;
    }

    private bool playerIsHoldingForward()
    {
        return player.moveDirection.x * orientation.forward.x > 0f || player.moveDirection.z * orientation.forward.z > 0f;
    }

    private void wallCheck()
    {
        Vector3 position = transform.position - new Vector3(0f, player.playerHeight * 0.25f, 0f);

        wallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, smallWall);
        wallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    private void tallWallCheck()
    {
        Vector3 position = transform.position + new Vector3(0f, player.playerHeight * 0.125f, 0f);

        tallWallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wall);
        tallWallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    private void startClimb()
    {
        player.state = PlayerControllerOffline.MovementState.climbing;
        player.speed = climbSpeed;
    }

    private void climbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        player.state = PlayerControllerOffline.MovementState.standing;
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
