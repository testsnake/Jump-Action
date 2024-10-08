using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    public Health playerHealth;
    public TextMeshProUGUI healthText;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("Player Health reference is missing!");
        }

        UpdateHealthDisplay();
    }

    void Update()
    {
        UpdateHealthDisplay();
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null && playerHealth != null)
        {
            healthText.text = playerHealth.getHealth().ToString();
        }
    }
}
