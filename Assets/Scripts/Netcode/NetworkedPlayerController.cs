using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
public class NetworkedPlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float speed;
    public float standingSpeed = 25f;
    public float jumpHeight = 5f;
    public float groundDrag = 2f;
    public float airDrag = 1f;
    public float airSpeedMultiplier = 0.4f;
    private Vector3 moveDirection;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask ground;
    private bool isGrounded;

    [Header("Crouching")]
    public float crouchingSpeed = 12f;
    public float crouchYScale = 0.5f;
    private float standYScale;

    [Header("SlopeHandling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;


    public Transform orientation;
    private InputActions inputActions;
    private InputAction movement;
    private Rigidbody rb;
    private MovementState state;
    public enum MovementState {
        standing,
        crouching,
        falling
    };

    public struct PlayerData : IComponentData
    {
        public float speed;
        public float standingSpeed;
        public float jumpHeight;
        public float groundDrag;
        public float airDrag;
        public float airSpeedMultiplier;
        public float playerHeight;
        public bool isGrounded;
        public float crouchingSpeed;
        public float crouchYScale;
        public float standYScale;
        public float maxSlopeAngle;
    }

    public class PlayerBaker : Baker<NetworkedPlayerController>
    {
        public override void Bake(NetworkedPlayerController authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerData
            {
                speed = authoring.speed,
                standingSpeed = authoring.standingSpeed,
                jumpHeight = authoring.jumpHeight,
                groundDrag = authoring.groundDrag,
                airDrag = authoring.airDrag,
                airSpeedMultiplier = authoring.airSpeedMultiplier,
                playerHeight = authoring.playerHeight,
                isGrounded = authoring.isGrounded,
                crouchingSpeed = authoring.crouchingSpeed,
                crouchYScale = authoring.crouchYScale,
                standYScale = authoring.standYScale,
                maxSlopeAngle = authoring.maxSlopeAngle
            });
            AddComponent<PlayerInputData>(entity);
        }
    }
    
    //private void Awake()
    //{
    //    rb = GetComponent<Rigidbody>();
    //    rb.freezeRotation = true;
    //    inputActions = new InputActions();
    //    movement = inputActions.Player.Movement;
    //    standYScale = transform.localScale.y;
    //    speed = standingSpeed;
    //    state = MovementState.standing;
    //}

    //private void Update()
    //{
    //    checkGrounded();
    //    limitSpeed();   
    //}

    //private void FixedUpdate()
    //{
    //    movePlayer();
    //}

    ////called when script enabled
    //private void OnEnable()
    //{
    //    movement.Enable();
    //    inputActions.Player.Jump.performed += Jump;
    //    inputActions.Player.Jump.Enable();
    //    inputActions.Player.Crouch.performed += Crouch;
    //    inputActions.Player.Crouch.canceled += Uncrouch;
    //    inputActions.Player.Crouch.Enable();
    //}
    ////called when script disabled
    //private void OnDisable()
    //{
    //    movement.Disable();
    //    inputActions.Player.Jump.performed -= Jump;
    //    inputActions.Player.Jump.Disable();
    //    inputActions.Player.Crouch.performed -= Crouch;
    //    inputActions.Player.Crouch.canceled -= Uncrouch;
    //    inputActions.Player.Crouch.Disable();
    //}

    //private void Jump(InputAction.CallbackContext obj)
    //{
    //    if (isGrounded)
    //    {
    //        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    //        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    //    }
    //}

    //private void Crouch(InputAction.CallbackContext obj)
    //{
    //    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
    //    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    //    speed = crouchingSpeed;
    //    state = MovementState.crouching;
    //}

    //private void Uncrouch(InputAction.CallbackContext obj)
    //{
    //    transform.localScale = new Vector3(transform.localScale.x, standYScale, transform.localScale.z);
    //    speed = standingSpeed;
    //    state = MovementState.standing;
    //}

    //private void movePlayer()
    //{
    //    Vector2 v2 = movement.ReadValue<Vector2>();
    //    moveDirection = orientation.forward * v2.y + orientation.right * v2.x;

    //    if (onSlope())
    //    {
    //        rb.AddForce(getSlopeMovementDirection() * speed, ForceMode.Force);
    //    }
        
    //    if (isGrounded)
    //        rb.AddForce(moveDirection.normalized * speed, ForceMode.Force);
    //    else
    //        rb.AddForce(moveDirection.normalized * speed * airSpeedMultiplier, ForceMode.Force);
    //}

    //private void limitSpeed()
    //{
    //    Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

    //    if (velocity.magnitude > speed)
    //    {
    //        Vector3 limitedVelocity = velocity.normalized * speed;
    //        rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
    //    }
    //}

    //private void checkGrounded()
    //{
    //    isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

    //    if (isGrounded)
    //    {
    //        rb.drag = groundDrag;
    //        if(state == MovementState.falling)
    //            state = MovementState.standing;
    //    }
    //    else
    //    {
    //        rb.drag = airDrag;
    //        state = MovementState.falling;
    //    }
    //}

    //private bool onSlope()
    //{
    //    if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
    //    {
    //        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
    //        return angle < maxSlopeAngle && angle != 0;
    //    }

    //    return false;
    //}

    //private Vector3 getSlopeMovementDirection()
    //{
    //    return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    //}
}
