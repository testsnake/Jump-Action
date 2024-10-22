using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector]
    public float speed;
    public float standingSpeed = 10f;
    public float jumpHeight = 5f;
    public float groundDrag = 2f;
    public float airDrag = 1f;
    public float airSpeedMultiplier = 0.4f;

    [HideInInspector]
    public Vector3 moveDirection;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask ground;
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
    private WallRunning wallRunning;

    [Header("Climbing")]
    private Climbing climbing;

    [Header("SlopeHandling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;
    public Transform orientation;
    private InputActions inputActions;
    private InputAction movement;
    [HideInInspector]
    public Rigidbody rb;
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputActions = new InputActions();
        movement = inputActions.Player.Movement;
        standYScale = transform.localScale.y;
        speed = standingSpeed;
        state = MovementState.standing;
        slideTimer = maxSlideTime;
        wallRunning = GetComponent<WallRunning>();
        climbing = GetComponent<Climbing>();
    }

    private void Update()
    {
        checkGrounded();
        limitSpeed();
    }

    private void FixedUpdate()
    {
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

    //called when script enabled
    private void OnEnable()
    {
        movement.Enable();
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.Jump.Enable();
        inputActions.Player.Crouch.performed += Crouch;
        inputActions.Player.Crouch.canceled += Uncrouch;
        inputActions.Player.Crouch.Enable();
    }
    //called when script disabled
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
        }
        else if (state == MovementState.wallRunning)
        {
            wallRunning.wallJump();
        }
        // else if (state == MovementState.climbing)
        // {
        //     climbing.climbJump();
        // }
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
        if (state != MovementState.sliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
            state = MovementState.standing;
        }
    }

    private void startSlide()
    {
        state = MovementState.sliding;
        slideTimer = maxSlideTime;
        Vector2 v2 = movement.ReadValue<Vector2>();
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;
    }

    private void slideMovement()
    {
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
        transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);

        state = isGrounded ? MovementState.standing : MovementState.falling;
    }

    private void movePlayer()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        moveDirection = orientation.forward * v2.y + orientation.right * v2.x;

        if (onSlope())
        {
            rb.AddForce(getSlopeMovementDirection(moveDirection) * speed * 10f, ForceMode.Force);
        }

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * speed * 10f * airSpeedMultiplier, ForceMode.Force);

        if (state != MovementState.wallRunning)
            rb.useGravity = !onSlope();
    }

    private void limitSpeed()
    {
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.4f, ground);

        if (isGrounded)
        {
            rb.drag = groundDrag;
            if (state == MovementState.falling)
                state = MovementState.standing;
        }
        else
        {
            rb.drag = airDrag;
            if (state != MovementState.sliding && state != MovementState.wallRunning)
                state = MovementState.falling;
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