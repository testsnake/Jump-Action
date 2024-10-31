using UnityEngine;
using TMPro;  

public class AmmoDisplay : MonoBehaviour
{
    public Ammo playerAmmo;  
    public TextMeshProUGUI ammoText;  

    void Start()
    {
        if (playerAmmo == null)
        {
            Debug.LogWarning("Player Ammo reference is missing!");
        }

        UpdateAmmoDisplay();
    }

    void Update()
    {
        UpdateAmmoDisplay();
    }

    void UpdateAmmoDisplay()
    {
        if (ammoText != null && playerAmmo != null)
        {
            ammoText.text = playerAmmo.getAmmo().ToString();
        }
    }
}
