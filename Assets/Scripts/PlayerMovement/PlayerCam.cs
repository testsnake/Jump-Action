using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    private InputActions inputActions;
    private InputAction rotation;
    public float turnSensitivity = 400f;
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
        Vector2 rotationVector2D = rotation.ReadValue<Vector2>();
        float x = rotationVector2D.x * turnSensitivity * Time.deltaTime;
        float y = rotationVector2D.y * turnSensitivity * Time.deltaTime;

        yRotation += x;
        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}
