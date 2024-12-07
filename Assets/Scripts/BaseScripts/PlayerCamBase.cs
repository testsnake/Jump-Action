using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.Netcode;

public class PlayerCamBase : MonoBehaviour
{
    [Header("Camera Settings")]
    public float turnSensitivity = 100f;
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset of the camera from the player

    [Header("References")]
    private Camera mainCamera;
    public Transform orientation;

    private InputActions inputActions;
    private InputAction rotation;

    protected float xRotation;
    protected float yRotation;
    protected float teamSpawnOffset = 0;

    private Tween fovTween;
    private Tween tiltTween;
    private Tween rotateTween;

    protected virtual void Awake()
    {
        inputActions = new InputActions();
        rotation = inputActions.Player.Rotation;
        turnSensitivity *= PlayerPrefs.GetFloat("MouseSens", 1f);
        mainCamera = GetComponentInChildren<Camera>();
        string team = PlayerPrefs.GetString("Team");
        if(team == "Blue")
        {
            teamSpawnOffset = 180;
        }
        AssignOrientation();
    }

    protected virtual void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (orientation == null)
        {
            Debug.LogWarning("orientation is still null in Start(). Calling AssignOrientation()");
            AssignOrientation();
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
            Debug.LogWarning("Orientation still in the Update null.");
            AssignOrientation();
            return;
        }
        else 
        {
            //Debug.LogWarning("Orientation is no longer null.");
        }

        // Handle rotation input
        Vector2 rotationVector2D = rotation.ReadValue<Vector2>();
        float x = rotationVector2D.x * turnSensitivity * Time.deltaTime;
        float y = rotationVector2D.y * turnSensitivity * Time.deltaTime;

        yRotation += x;
        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.position = orientation.position + offset;
        transform.rotation = Quaternion.Euler(xRotation, yRotation + teamSpawnOffset, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation + teamSpawnOffset, 0);
    }

    private void AssignOrientation()
    {
        // Find all players with the "Player" tag
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<NetworkObject>()?.IsOwner == true && PlayerPrefs.GetString("Mode") == "Online")
            {
                orientation = player.transform;
                return;
            }
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
