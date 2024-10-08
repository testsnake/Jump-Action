using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    private int playerStartAmmo = 30;
    private int ammo;

    void Start()
    {
        // Subject to change
        SetAmmo(playerStartAmmo);
    }

    void Update()
    {
        // TODO: Add logic regarding using up ammo and reloading
    }

    public void SetAmmo(int newAmmo)
    {
        ammo = newAmmo;
    }

    public int getAmmo()
    {
        return ammo;
    }

    public void GainAmmo(int ammoGainAmount)
    {
        int newAmmo = ammo + ammoGainAmount;
        SetAmmo(newAmmo);
    }

    public void LoseAmmo(int ammoLossAmount)
    {
        int newAmmo = ammo - ammoLossAmount;
        SetAmmo(newAmmo);
    }
}
