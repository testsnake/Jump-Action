using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.Netcode;

public class PlayerCamBase : MonoBehaviour
{
    [Header("Camera Settings")]
    public float turnSensitivity = 100f;
    public Vector3 offset = new Vector3(0, 1, 0); // Offset of the camera from the player

    [Header("References")]
    private Camera mainCamera;
    public Transform orientation;

    private InputActions inputActions;
    private InputAction rotation;

    protected float xRotation;
    protected float yRotation;

    private Tween fovTween;
    private Tween tiltTween;
    private Tween rotateTween;

    protected virtual void Awake()
    {
        inputActions = new InputActions();
        rotation = inputActions.Player.Rotation;
        turnSensitivity *= PlayerPrefs.GetFloat("MouseSens", 1f);

        AssignOrientation(); // Assign orientation if not manually set

        // Assign the mainCamera reference
        mainCamera = GetComponentInChildren<Camera>(); // Look for a Camera in child objects
    }

    protected virtual void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (orientation == null)
        {
            Debug.LogError("Orientation is not assigned! Make sure to assign it in the Inspector or dynamically in the code.");
        }
    }

    protected virtual void OnEnable()
    {
        rotation.Enable();
    }

    protected virtual void OnDisable()
    {
        rotation.Disable();
    }

    protected virtual void Update()
    {
        if (orientation == null)
        {
            Debug.LogWarning("Orientation is null. Attempting to reassign.");
            AssignOrientation();
            return;
        }

        // Handle rotation input
        Vector2 rotationVector2D = rotation.ReadValue<Vector2>();
        float x = rotationVector2D.x * turnSensitivity * Time.deltaTime;
        float y = rotationVector2D.y * turnSensitivity * Time.deltaTime;

        yRotation += x;
        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    protected virtual void LateUpdate()
    {
        // Ensure the camera follows the player
        if (orientation != null)
        {
            transform.position = orientation.position + offset; // Follow the player's position with an offset
        }
    }

    private void AssignOrientation()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<NetworkObject>()?.IsOwner == true && PlayerPrefs.GetString("Mode") == "Online")
            {
                orientation = player.transform;
                break;
            }
        }

        if (orientation == null)
        {
            Debug.LogError("Failed to assign orientation. Ensure the player has a NetworkObject and is properly tagged.");
        }
    }

    public virtual void DoFov(float endValue)
    {
        if (fovTween != null && fovTween.IsActive())
        {
            fovTween.Kill();
        }

        fovTween = mainCamera.DOFieldOfView(endValue, 0.25f)
                             .SetEase(Ease.OutQuad);
    }

    public virtual void DoTilt(float zTilt)
    {
        if (tiltTween != null && tiltTween.IsActive())
        {
            tiltTween.Kill();
        }

        // Only modify the z-axis for tilt
        tiltTween = mainCamera.transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f)
                             .SetEase(Ease.OutQuad);
    }

    public virtual void Rotate180(float duration)
    {
        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
        }

        rotateTween = transform.DORotate(new Vector3(0f, 180f, 0f), duration, RotateMode.LocalAxisAdd)
                             .SetEase(Ease.InOutQuad);
    }
}
