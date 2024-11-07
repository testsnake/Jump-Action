using System.Collections;
using System.Collections.Generic;
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
    float xRotation;
    float yRotation;

    private void Awake()
    {
        inputActions = new InputActions();
        rotation = inputActions.Player.Rotation;
    }

    private void OnEnable()
    {
        rotation.Enable();
    }
    //called when script disabled
    private void OnDisable()
    {
        rotation.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (orientation == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in allPlayers)
            {
                if (player.GetComponent<NetworkObject>()?.IsOwner == true)
                {
                    orientation = player.transform;
                }
            }
        } else
        {
            Vector2 rotationVector2D = rotation.ReadValue<Vector2>();
            float x = rotationVector2D.x * turnSensitivity * Time.deltaTime;
            float y = rotationVector2D.y * turnSensitivity * Time.deltaTime;

            yRotation += x;
            xRotation -= y;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void Rotate180(float duration)
    {
        transform.DORotate(new Vector3(0f, 180f, 0f), duration, RotateMode.LocalAxisAdd);
    }
}
