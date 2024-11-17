using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.Netcode;

public class PlayerCam : MonoBehaviour
{
    private InputActions inputActions;
    private InputAction rotation;

    public float turnSensitivity = 100f;
    public Transform orientation;
    public Transform camHolder;

    private float xRotation;
    private float yRotation;

    private Tween fovTween;
    private Tween tiltTween;
    private Tween rotateTween;

    private void Awake()
    {
        inputActions = new InputActions();
        rotation = inputActions.Player.Rotation;

        AssignOrientation(); // Assign orientation at the start
    }

    private void Start()
    {
        if (camHolder == null)
        {
            Debug.LogError("camHolder is not assigned in PlayerCam. Please assign it in the Inspector.");
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        rotation.Enable();
    }

    private void OnDisable()
    {
        rotation.Disable();
    }

    private void Update()
    {
        if (orientation == null)
        {
            Debug.LogWarning("Orientation is null. Attempting to reassign.");
            AssignOrientation();
            return;
        }

        Vector2 rotationVector2D = rotation.ReadValue<Vector2>();
        float x = rotationVector2D.x * turnSensitivity * Time.deltaTime;
        float y = rotationVector2D.y * turnSensitivity * Time.deltaTime;

        yRotation += x;
        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void AssignOrientation()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            if (player.GetComponent<NetworkObject>()?.IsOwner == true)
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

    public void DoFov(float endValue)
    {
        if (fovTween != null && fovTween.IsActive())
        {
            fovTween.Kill();
        }

        fovTween = GetComponent<Camera>().DOFieldOfView(endValue, 0.25f)
                                         .SetEase(Ease.OutQuad);
    }

    public void DoTilt(float zTilt)
    {
        if (tiltTween != null && tiltTween.IsActive())
        {
            tiltTween.Kill();
        }

        tiltTween = transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f)
                             .SetEase(Ease.OutQuad);
    }

    public void Rotate180(float duration)
    {
        if (rotateTween != null && rotateTween.IsActive())
        {
            rotateTween.Kill();
        }

        rotateTween = transform.DORotate(new Vector3(0f, 180f, 0f), duration, RotateMode.LocalAxisAdd)
                               .SetEase(Ease.InOutQuad);
    }
}
