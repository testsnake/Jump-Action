using UnityEngine;

public class PlayerShootBase : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    public Transform playerCam; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.0f; // Distance in front of the player

    public virtual void Start()
    {
        // Find the main camera to use as the shooting direction (can be overridden in derived classes)
        if (playerCam == null)
        {
            playerCam = GameObject.FindWithTag("MainCamera")?.GetComponent<Transform>();
        }
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
        // Instantiate the projectile at the player's position and rotation (can be overridden for custom behavior)
        if (projectilePrefab != null && playerCam != null)
        {
            Instantiate(projectilePrefab, transform.position + playerCam.forward * firePointDistance, playerCam.rotation);
        }
        else
        {
            Debug.LogWarning("ProjectilePrefab or PlayerCam is not assigned in PlayerShootBase.");
        }
    }
}
