using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private int playerStartHealth = 100;
    private int health;

    void Start()
    {
        // Subject to change
        SetHealth(playerStartHealth);
    }

    void Update()
    {
        // TODO: Add logic regarding taking damage
    }

    public void SetHealth(int newHealth)  
    { 
        health = newHealth;
    }

    public int getHealth()
    { 
        return health; 
    }

    public void GainHealth(int healthGainAmount)
    {
        int newHealth = health + healthGainAmount;
        SetHealth(newHealth);
    }

    public void LoseHealth(int healthLossAmount)
    { 
        int newHealth = health - healthLossAmount;
        SetHealth(newHealth);
    }

    
}
