using UnityEngine;
using Unity.Netcode;

public class PlayerShoot : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public Transform playerCam; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.0f; // Distance in front of the player

    void Start()
    {
        if (!IsOwner) return;
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
    }

    void Update()
    {
        if (!IsOwner) return;
        // Fire the projectile when left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (!IsOwner) return;
        // Instantiate the projectile at the fire point's position
        GameObject projectile = Instantiate(projectilePrefab, transform.position, playerCam.rotation);
    }



}
