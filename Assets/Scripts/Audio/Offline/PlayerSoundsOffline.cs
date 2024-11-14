using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSoundsOffline : MonoBehaviour
{
    public GameObject playerObject;
    private Rigidbody playerRB;
    private PlayerController playerMovement;
    public AudioSource runningSound;
    public AudioSource wallRunningSound;
    public AudioSource jumpingSound;
    public AudioSource slidingSound;
    public AudioSource dyingSound;
    public AudioSource shootingSound;
    public AudioSource grabChipSound;
    private Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();

    void Start()
    {
        sounds.Add("Run", runningSound);
        sounds.Add("Wall Run", wallRunningSound);
        sounds.Add("Jump", jumpingSound);
        sounds.Add("Slide", slidingSound);
        sounds.Add("Die", dyingSound);
        sounds.Add("Shoot", shootingSound);
        sounds.Add("Grab Chip", grabChipSound);
    }

    void Update()
    {
        if (playerRB == null) playerRB = playerObject.GetComponent<Rigidbody>();
        else if (playerMovement == null) playerMovement = playerObject.GetComponent<PlayerController>();
        else
        {
            bool currentlyRunning = isRunning();
            bool currentlyWallRunning = isWallRunning();
            bool currentlySliding = isSliding();

            if (currentlyRunning && !runningSound.isPlaying)
                playSound("Run");
            else if (!currentlyRunning && runningSound.isPlaying)
                stopSound("Run");

            if (currentlyWallRunning && !wallRunningSound.isPlaying)
                playSound("Wall Run");
            else if (!currentlyWallRunning && wallRunningSound.isPlaying)
                stopSound("Wall Run");

            if (!currentlySliding && slidingSound.isPlaying)
                stopSound("Slide");
        }
    }

    bool isRunning()
    {
        return (playerMovement.moveDirection.x != 0f || playerMovement.moveDirection.z != 0f) && 
                playerMovement.state == PlayerController.MovementState.standing;
    }

    bool isWallRunning()
    {
        return (playerMovement.moveDirection.x != 0f || playerMovement.moveDirection.z != 0f) && 
                playerMovement.state == PlayerController.MovementState.wallRunning;
    }

    bool isSliding()
    {
        return playerMovement.state == PlayerController.MovementState.sliding;
    }

    public void playSound(string soundName)
    {
        if (sounds.ContainsKey(soundName))
            sounds[soundName].Play();
    }

    public void stopSound(string soundName)
    {
        if (sounds.ContainsKey(soundName) && sounds[soundName].isPlaying)
            sounds[soundName].Stop();
    }
}
