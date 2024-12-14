using UnityEngine;
using Unity.Netcode;
using UnityEditor;

public class Projectile : NetworkBehaviour
{
    public float speed;
    public float lifetime;
    public float damage = 25f;
    public ulong ownerClientId;
    
    
    private string _team;
    private bool isInitialized = false;

    public void Start()
    {
        Destroy(gameObject, lifetime);
    }

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
            Destroy(gameObject);
            return;
        }

        GameObject player = otherObject.transform.parent.gameObject;
        Health playerHealth = player.GetComponent<Health>();
        string playerTeam = playerHealth.GetTeam();

        if (_team == playerTeam)
        {
            Destroy(gameObject);
            return;
        }

        playerHealth.ApplyDamage(damage);
        onTriggerEnterMessage += "|| The Other Object was an Enemy Player!";
        Debug.Log(onTriggerEnterMessage);

        Destroy(gameObject);
    }
}
