using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f; // Damage dealt by this projectile
    public ulong ownerClientId; // The owner of this projectile (NetworkObjectId)

    private string _team;

    void Start()
    {
        if (IsServer)
        {
            // Schedule destruction on the server
            Invoke(nameof(DestroyNetworkObject), lifetime);
        }
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
        // Call ServerRpc to handle collision logic on the server
        HandleCollisionServerRpc(other.GetComponent<NetworkObject>()?.NetworkObjectId ?? 0, other.GetComponent<Health>() != null);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleCollisionServerRpc(ulong targetNetworkObjectId, bool hasHealth)
    {
        // Only the server processes the collision logic
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetNetworkObjectId)
            ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId]
            : null;

        if (targetNetworkObject != null)
        {
            // Ignore if the target is the owner of the projectile
            if (targetNetworkObject.OwnerClientId == ownerClientId)
            {
                Debug.Log("Projectile hit its owner; ignoring.");
                return;
            }
        }

        // Check if the object has a Health component
        if (hasHealth && targetNetworkObject != null)
        {
            Health targetHealth = targetNetworkObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                // Damage the target
                targetHealth.ApplyDamage(damage);

                // Optionally log the hit
                Debug.Log($"{gameObject.name} hit {targetNetworkObject.name} for {damage} damage.");
            }
        }

        // Destroy the projectile on impact
        DestroyNetworkObject();
    }

    private void DestroyNetworkObject()
    {
        if (IsServer)
        {
            // Despawn and destroy the network object
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
