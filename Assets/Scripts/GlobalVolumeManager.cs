using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeManager : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Vignette")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float vignetteMaxIntensity = 0.5f;
    [SerializeField] private float vignetteScale = 0.5f;
    [SerializeField] private float hurtLengthSeconds = 0.9f;
    [SerializeField] private float hurtMultiplier = 0.5f;
    private float maxHealth = 100f;

    [Header("Chromatic Aberration")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float runAberration = 0.1f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float maxAberration = 1f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float verticalMultiplier = 1f;

    private bool directMode = false;
    private float directVignette = 0;
    private float directChromatic = 0;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;



    private float health = 0;
    private float speed = 0;
    private double timeOfLastDamage = -1;

    public void UpdateHealth(float health)
    {
        // User has taken damage
        if (this.health > health)
        {
            timeOfLastDamage = Time.timeAsDouble;
        }
        this.health = health;

    }

    public void UpdateSpeed(Vector3 vector)
    {
        this.speed = vector.magnitude;
    }

    void Update()
    {
        if (chromaticAberration == null || vignette == null)
        {
            if (!globalVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = globalVolume.profile.Add<ChromaticAberration>();
            }

            if (!globalVolume.profile.TryGet(out vignette))
            {
                vignette = globalVolume.profile.Add<Vignette>();
            }
        }
        else
        {
            UpdateVolume();
        }
    }


    private void UpdateVolume()
    {
        if (!directMode)
        {
            UpdateVignette();
            UpdateAberration();
        } else {
            chromaticAberration.intensity.value = directChromatic;
            vignette.intensity.value = directVignette;
        }

    }

    private void UpdateVignette()
    {
        double sinceLastDamage = Time.timeAsDouble - timeOfLastDamage;

        double multiplier = Math.Max(((sinceLastDamage / hurtLengthSeconds) - hurtLengthSeconds) * -1, 0) * hurtMultiplier;

        double damage = (maxHealth - health) / maxHealth;

        double baseIntensity = Math.Pow(damage, vignetteScale);


        double intensity = (baseIntensity + multiplier) * vignetteMaxIntensity;

        vignette.intensity.value = (float)intensity;
    }

    private void UpdateAberration()
    {
        chromaticAberration.intensity.value = ComputeAberration(speed);
    }

    private float ComputeAberration(float speed)
    {
        if (speed <= 0)
            return 0f;

        if (speed <= runSpeed)
        {
            // Normalize speed to [0, 1] for the range [0, runSpeed]
            float normalizedSpeed = speed / runSpeed;
            // Exponentially scale between 0 and runAberration
            return Mathf.Lerp(0f, runAberration, Mathf.Pow(normalizedSpeed, 2f)); // Adjust exponent as needed
        }
        else if (speed <= maxSpeed)
        {
            // Normalize speed to [0, 1] for the range [runSpeed, maxSpeed]
            float normalizedSpeed = (speed - runSpeed) / (maxSpeed - runSpeed);
            // Exponentially scale between runAberration and maxAberration
            return Mathf.Lerp(runAberration, maxAberration, Mathf.Pow(normalizedSpeed, 2f)); // Adjust exponent as needed
        }
        else
        {
            // Clamp to maxAberration if speed exceeds maxSpeed
            return maxAberration;
        }
    }

    public void SetDirectMode(bool mode) {
        directMode = mode;
    }

    public void SetChromaticAberration(float value) {
        directChromatic = value;
    }

    public float GetChromaticValue() {
        return chromaticAberration.intensity.value;
    }

    public void SetVingette(float value) {
        directVignette = value;
    }
}
