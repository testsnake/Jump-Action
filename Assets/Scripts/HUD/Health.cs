using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public string team; // Assign team dynamically (e.g., "Blue" or "Red")

    public GlobalVolumeManager globalVolumeManager;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (globalVolumeManager != null)
        {
            globalVolumeManager.UpdateHealth(currentHealth);
        }
        else
        {
            GameObject gvm = GameObject.FindWithTag("GameManager");
            if (gvm != null)
            {
                globalVolumeManager = gvm.GetComponent<GlobalVolumeManager>();
            }
        }
    }

    public string GetTeam()
    {
        team = PlayerPrefs.GetString("Team");
        return team;
    }


    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");
        globalVolumeManager?.UpdateHealth(currentHealth);
        if (globalVolumeManager == null) {
            Debug.Log("NULL");
        }
        if (currentHealth <= 0)
        {
            Die();
            ResetHealth();
        }

        
    }

    private void ResetHealth()
    {
        currentHealth = maxHealth;
    }

/*    private void OnCollisionEnter(Collision other)
    {


        string collisionMessage = "OnCollisionEnter():";

        Debug.Log(collisionMessage);
        ApplyDamage(20f);
        GameObject projectileGameObject = other.gameObject;

        Destroy(projectileGameObject);
        *//*Projectile projectile = projectileGameObject.GetComponent<Projectile>();
        projectile.HandleCollisionServerRpc();*//*

    }*/

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        // Add respawn or death logic here
        PlayerControllerBase playerController = GetComponent<PlayerControllerBase>();
        if (playerController != null)
        {
            playerController.Die(); // Use existing Die() method in PlayerControllerBase
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
