using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Collections.ObjectModel;
using System.Linq;



public class Projectile : NetworkBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f;
    public ulong ownerClientId;
    
    
    private string _team;
    private bool isInitialized = false;

    /*private void Start()
    {
        if (IsServer && isInitialized)
        {
            Invoke(nameof(DestroyNetworkObject), lifetime);
        }
    }*/

    /*public void InitializeLifetime()
    {
        if (!IsServer) return;

        isInitialized = true;
        Invoke(nameof(DestroyNetworkObject), lifetime);
    }*/

    private void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    public void InitializeTeam(string team)
    {
        _team = team;
        isInitialized = true;

        Debug.Log("Projectile Initialized.");
    }

    public string GetTeam()
    {
        return _team;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;

        string onTriggerEnterMessage = "OnTriggerEnter(): ";
        GameObject otherObject = other.gameObject;

        if (otherObject.tag != "Player") 
        {
            Destroy(this);
            return;
        }

        GameObject player = otherObject.transform.parent.gameObject;
        Health playerHealth = player.GetComponent<Health>();
        string playerTeam = playerHealth.GetTeam();

        if (_team == playerTeam)
        {
            Destroy(this);
            return;
        }

        playerHealth.ApplyDamage(damage);
        onTriggerEnterMessage += "|| The Other Object was an Enemy Player!";
        Debug.Log(onTriggerEnterMessage);

        /*HandleCollisionServerRpc();*/
        Destroy(this);
    }

    /*[ServerRpc(RequireOwnership = false)]
    public void HandleCollisionServerRpc()
    {
        Debug.Log("HandleCollisionServerRpc() Got Called!");
        Debug.Log("Calling CalledClientRpc");
        CalledClientRpc();

        if (!isInitialized) return;

        *//*NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetNetworkObjectId)
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
        }*//*

        DestroyNetworkObject();
    }*/

    /*[ClientRpc]
    public void CalledClientRpc()
    {
        Debug.Log("HandleCollisionServerRpc() Got Called!");
    }*/

    /*public void DestroyNetworkObject()
    {
        NetworkObject projectileNetwork = GetComponent<NetworkObject>();
        if (IsServer && projectileNetwork.IsSpawned)
        {
            projectileNetwork.Despawn(true);
        }
    }*/
}

