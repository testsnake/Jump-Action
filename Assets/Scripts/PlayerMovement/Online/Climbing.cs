using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class Climbing : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation; // Reference to player's Orientation
    private Rigidbody rb; // Reference to player's rigidbody
    public LayerMask wall; // Reference to objects that can be run up
    public LayerMask smallWall; // Reference to objects that can be vaulted over 
    private PlayerController player; // Reference to player's movement
    public PlayerCam cam;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    [Header("WallDetection")]
    public float detectionLength;
    public float sphereCastRadius;

    // Max angle at which the spherecast collision will be valid (Makes it so that player has to be somewhat facing the wall)
    public float maxAngle;
    private float wallAngle;
    private float tallWallAngle;
    private RaycastHit frontWallHit;
    private RaycastHit tallWallHit;
    private bool wallFront;
    private bool tallWallFront;


    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerController>();
        orientation = transform.Find("Orientation");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        wallCheck();
        tallWallCheck();

        if ((wallFront || tallWallFront) && playerIsHoldingForward() && (wallAngle < maxAngle || tallWallAngle < maxAngle)) // Player is running into wall or barrier
        {
            if (canClimb() || canVault())
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

    private bool canClimb()
    {
        return tallWallFront && player.state == PlayerController.MovementState.falling && climbTimer > 0;
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
        if (!IsOwner) return;
        // Shoot raycast from a bit below the center of the player
        Vector3 position = transform.position - new Vector3(0f, player.playerHeight * 0.25f, 0f);

        wallFront = Physics.SphereCast(position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, smallWall);
        wallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    private void tallWallCheck()
    {
        if (!IsOwner) return;
        // Shoot raycast from a bit above the center of the player
        Vector3 position = transform.position + new Vector3(0f, player.playerHeight * 0.125f, 0f);

        tallWallFront = Physics.SphereCast(position, sphereCastRadius + 0.25f, orientation.forward, out frontWallHit, detectionLength, wall);
        tallWallAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (player.isGrounded)
            climbTimer = maxClimbTime;
    }

    private void startClimb()
    {
        if (!IsOwner) return;
        player.state = PlayerController.MovementState.climbing;
        player.speed = climbSpeed;
    }

    private void climbingMovement()
    {
        if (!IsOwner) return;
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        if (!IsOwner) return;
        player.state = PlayerController.MovementState.standing;
        player.speed = player.standingSpeed;
    }
}
