using UnityEngine;
using Unity.Netcode;

public class PlayerShootBase : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    private Transform cameraHolder; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.1f; // Distance in front of the player

    public virtual void Start()
    {
        if (!IsOwner) return;

        // Find the main camera to use as the shooting direction (can be overridden in derived classes)
        cameraHolder = GameObject.Find("CameraHolder")?.GetComponent<Transform>();
    }

    public virtual void Update()
    {
        if (!IsOwner) return;

        // Fire the projectile when the left mouse button is clicked (can be overridden)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public virtual void Shoot()
    {

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
