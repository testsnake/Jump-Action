using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerShootBase : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    private Transform cameraHolder; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.1f; // Distance in front of the player

    private InputActions inputActions;

    public void Awake()
    {
        inputActions = new InputActions();
    }

    public virtual void Start()
    {
        if (!IsOwner) return;
        // Find the main camera to use as the shooting direction (can be overridden in derived classes)
        cameraHolder = GameObject.Find("CameraHolder")?.GetComponent<Transform>();
    }

    public virtual void Update()
    {
        if (!IsOwner) return;
    }

    private void OnEnable()
    {
        inputActions.Player.Shoot.performed += Shoot;
        inputActions.Player.Shoot.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Shoot.performed -= Shoot;
        inputActions.Player.Shoot.Disable();
    }

    private void Shoot(InputAction.CallbackContext obj)
    {
        if(!IsOwner) return;
        if (projectilePrefab != null && cameraHolder != null)
        {
            Vector3 spawnPosition = cameraHolder.position + cameraHolder.forward * firePointDistance;
            Quaternion spawnRotation = cameraHolder.rotation;

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

            if (projectile != null)
            {
                // Assign the ownerClientId to the projectile
                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.ownerClientId = NetworkManager.Singleton.LocalClientId;
                }
            }
        }
        else
        {
            Debug.LogWarning("ProjectilePrefab or PlayerCam is not assigned in PlayerShootBase.");
        }
    }
}
