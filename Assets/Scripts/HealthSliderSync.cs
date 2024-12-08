using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthSliderSync : MonoBehaviour
{
    [SerializeField] private Slider healthSlider; 
    private Health targetHealth;                 

    private void Start()
    {
        AssignPlayerHealth();
    }

    private void AssignPlayerHealth()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            var health = player.GetComponent<Health>();
            if (health != null && player.GetComponent<NetworkObject>()?.IsOwner == true)
            {
                targetHealth = health;
                InitializeSlider();
                return;
            }
        }

        Debug.LogWarning("Could not find the local player's Health component.");
    }

    private void InitializeSlider()
    {
        if (targetHealth == null || healthSlider == null)
        {
            Debug.LogError("Health component or slider is not assigned.");
            return;
        }

        healthSlider.maxValue = targetHealth.maxHealth;
        healthSlider.value = targetHealth.GetCurrentHealth();
    }

    private void Update()
    {
        if (!targetHealth)
        {
            AssignPlayerHealth();
            return;
        }
        if (targetHealth != null && healthSlider != null)
        {
            healthSlider.value = targetHealth.GetCurrentHealth();
        }
    }
}
