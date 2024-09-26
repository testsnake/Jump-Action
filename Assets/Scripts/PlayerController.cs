using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;
    private InputAction movement;
    private Rigidbody rb;
    public float topSpeed = 10f;
    public float speed = 1f;
    public float jumpHeight = 5f;
    public float turnSensitivity = 10f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputActions();
        movement = inputActions.Player.Movement;
    }
    //called when script enabled
    private void OnEnable()
    {
        movement.Enable();
        inputActions.Player.Jump.performed += Jump;
        inputActions.Player.Jump.Enable();
    }
    //called when script disabled
    private void OnDisable()
    {
        movement.Disable();
        inputActions.Player.Jump.performed -= Jump;
        inputActions.Player.Jump.Disable();
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        Vector3 jumpVector = new Vector3(0f, 1f, 0f);
        rb.AddForce(jumpVector * jumpHeight, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        Vector3 v3 = new Vector3(v2.x, 0, v2.y);

        rb.AddForce(v3 * speed, ForceMode.VelocityChange);

        if (rb.velocity.magnitude > topSpeed)
        {
            rb.velocity = rb.velocity.normalized * topSpeed;
        }
    }
}
