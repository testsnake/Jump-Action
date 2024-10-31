using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform player; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.0f; // Distance in front of the player

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
        GameObject projectile = Instantiate(projectilePrefab, transform.position, player.rotation);
    }



}
