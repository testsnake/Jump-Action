using UnityEngine;
using Unity.Netcode;

public class PlayerShootOffline : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform playerCam; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.0f; // Distance in front of the player

    void Start()
    {
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
    }

    void Update()
    {
        // Fire the projectile when left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Instantiate the projectile at the fire point's position
        GameObject projectile = Instantiate(projectilePrefab, transform.position, playerCam.rotation);
    }



}
