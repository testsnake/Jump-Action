using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSounds : MonoBehaviour
{
    private GameObject playerObject; // Reference to player object
    private Rigidbody playerRB; // Reference to player rigidbody
    private PlayerController playerMovement; // Reference to Player Novement Script
    public AudioSource runningSound;
    public AudioSource wallRunningSound;
    public AudioSource jumpingSound;
    public AudioSource slidingSound;
    public AudioSource dyingSound;
    public AudioSource shootingSound;
    public AudioSource grabChipSound;
    private Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>(); // Dictionary linking strings (names) to audio sources

    void Start()
    {
        // Add audios to dictionary
        sounds.Add("Run", runningSound);
        sounds.Add("Wall Run", wallRunningSound);
        sounds.Add("Jump", jumpingSound);
        sounds.Add("Slide", slidingSound);
        sounds.Add("Die", dyingSound);
        sounds.Add("Shoot", shootingSound);
        sounds.Add("Grab Chip", grabChipSound);
    }

    public Dictionary<string, AudioSource> GetSounds()
    {
        return sounds;
    }

    void Update()
    {
        // Assign player object reference from networked object
        if (playerObject == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in allPlayers)
            {
                if (player.GetComponent<NetworkObject>()?.IsOwner == true)
                {
                    playerObject = player;
                }
            }
        }
        else if (playerRB == null) playerRB = playerObject.GetComponent<Rigidbody>();
        else if (playerMovement == null) playerMovement = playerObject.GetComponent<PlayerController>();
        else
        {
            bool currentlyRunning = isRunning();
            bool currentlyWallRunning = isWallRunning();
            bool currentlySliding = isSliding();

            if (currentlyRunning && !runningSound.isPlaying) // If player is running and running audio is not playing
                playSound("Run");
            else if (!currentlyRunning && runningSound.isPlaying) // If player is not running and running audio is playing
                stopSound("Run");

            if (currentlyWallRunning && !wallRunningSound.isPlaying) // If player is wallrunning and wallrunning audio is not playing
                playSound("Wall Run");
            else if (!currentlyWallRunning && wallRunningSound.isPlaying) // If player is not wallrunning and wallrunning audio is playing
                stopSound("Wall Run");

            if (!currentlySliding && slidingSound.isPlaying) // If player is not sliding and sliding audio is playing
                stopSound("Slide");
        }
    }

    bool isRunning()
    {
        // Player is moving and is in standing state (On the ground and not crouching or sliding)
        return (playerMovement.moveDirection.x != 0f || playerMovement.moveDirection.z != 0f) && 
                playerMovement.state == PlayerController.MovementState.standing;
    }

    bool isWallRunning()
    {
        // Player is moving and is in wallrunning state
        return (playerMovement.moveDirection.x != 0f || playerMovement.moveDirection.z != 0f) && 
                playerMovement.state == PlayerController.MovementState.wallRunning;
    }

    bool isSliding()
    {
        return playerMovement.state == PlayerController.MovementState.sliding;
    }

    public void playSound(string soundName)
    {
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume");
        if (sounds.ContainsKey(soundName))
        {
            sounds[soundName].volume = sfxVol;
            sounds[soundName].Play();
        }
            
    }

    public void stopSound(string soundName)
    {
        if (sounds.ContainsKey(soundName) && sounds[soundName].isPlaying)
            sounds[soundName].Stop();
    }
}
