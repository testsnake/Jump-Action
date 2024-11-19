using UnityEngine;

public class PlayerShootBase : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    private Transform cameraHolder; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.1f; // Distance in front of the player

    public virtual void Start()
    {
        // Find the main camera to use as the shooting direction (can be overridden in derived classes)
        if (cameraHolder == null)
        {
            cameraHolder = GameObject.Find("CameraHolder")?.GetComponent<Transform>();
        }
        if (cameraHolder == null)
            Debug.Log("Still NULL");
    }

    public virtual void Update()
    {
        // Fire the projectile when the left mouse button is clicked (can be overridden)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public virtual void Shoot()
    {
        Debug.Log("Shoot() Called!"); // Check if Shoot() is called

        if (projectilePrefab != null && cameraHolder != null)
        {
            Vector3 spawnPosition = cameraHolder.position + cameraHolder.forward * firePointDistance;
            Quaternion spawnRotation = cameraHolder.rotation;

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
            if (projectile != null)
            {
                Debug.Log("Projectile Created Successfully!"); // Check if the projectile was created
            }
        }
        else
        {
            Debug.LogWarning("ProjectilePrefab or PlayerCam is not assigned in PlayerShootBase.");
        }
    }

}
