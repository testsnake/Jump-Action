using UnityEngine;
using Unity.Netcode;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f; // Damage dealt by this projectile
    public ulong ownerClientId; // The owner of this projectile (NetworkObjectId)

    private string _team;

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy projectile after its lifetime
    }

    void FixedUpdate()
    {
        // Move the projectile forward
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    public void SetTeam(string team) 
    {
        _team = team;
    }

    public string GetTeam() 
    {
        return _team;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object hit has a NetworkObject
        NetworkObject targetNetworkObject = other.GetComponent<NetworkObject>();
        if (targetNetworkObject != null)
        {
            // Ignore if the target is the owner of the projectile
            if (targetNetworkObject.OwnerClientId == ownerClientId)
            {
                Debug.Log("Projectile hit its owner; ignoring.");
                return;
            }
        }

        // Check if the object hit has a Health component
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            // Damage the target
            targetHealth.ApplyDamage(damage);

            // Optionally log the hit
            Debug.Log($"{gameObject.name} hit {other.gameObject.name} for {damage} damage.");
        }

        // Destroy the projectile on impact
        Destroy(gameObject);
    }
}
