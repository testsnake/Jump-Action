using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f;
    public ulong ownerClientId;

    private string _team;
    private bool isInitialized = false;

    private void Start()
    {
        if (IsServer && isInitialized)
        {
            Invoke(nameof(DestroyNetworkObject), lifetime);
        }
    }

    public void InitializeLifetime()
    {
        if (!IsServer) return;

        isInitialized = true;
        Invoke(nameof(DestroyNetworkObject), lifetime);
    }

    private void FixedUpdate()
    {
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

/*    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;

        HandleCollisionServerRpc();
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void HandleCollisionServerRpc()
    {
        if (!isInitialized) return;

        /*NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetNetworkObjectId)
            ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId]
            : null;

        if (targetNetworkObject != null)
        {
            if (targetNetworkObject.OwnerClientId == ownerClientId)
            {
                Debug.Log("Projectile hit its owner; ignoring.");
                return;
            }
        }

        if (hasHealth && targetNetworkObject != null)
        {
            Health targetHealth = targetNetworkObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.ApplyDamage(damage);

                Debug.Log($"{gameObject.name} hit {targetNetworkObject.name} for {damage} damage.");
            }
        }*/

        DestroyNetworkObject();
    }

    public void DestroyNetworkObject()
    {
        NetworkObject projectileNetwork = GetComponent<NetworkObject>();
        if (IsServer && projectileNetwork.IsSpawned)
        {
            projectileNetwork.Despawn(true);
        }
    }
}

