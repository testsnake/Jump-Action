using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f; // Damage dealt by this projectile

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy projectile after its lifetime
    }

    void FixedUpdate()
    {
        // Move the projectile forward
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object hit has a Health component
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            // Damage the target regardless of their team
            targetHealth.ApplyDamage(damage);

            // Optionally log the hit
            Debug.Log($"{gameObject.name} hit {other.gameObject.name} for {damage} damage.");
        }

        // Destroy the projectile on impact
        Destroy(gameObject);
    }
}
