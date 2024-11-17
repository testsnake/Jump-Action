using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float lifetime;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void FixedUpdate() 
    {
        // Move the GameObject by velocity * time between frames
        transform.position += transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Add logic to deal damage to the target
        Debug.Log("Projectile hit " + other.gameObject.name);

        // Destroy the projectile
        Destroy(gameObject);
    }
}
