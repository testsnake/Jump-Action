using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerShootBase : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    private Transform cameraHolder; // Reference to the camera or look direction of the player
    public float firePointDistance = 1.1f; // Distance in front of the player

    private InputActions inputActions;

    public void Awake()
    {
        inputActions = new InputActions();
    }

    public virtual void Start()
    {
        if (!IsOwner) return;

        // Find the main camera to use as the shooting direction (can be overridden in derived classes)
        cameraHolder = GameObject.Find("CameraHolder")?.GetComponent<Transform>();
    }

    public virtual void Update()
    {
        if (!IsOwner) return;
    }

    private void OnEnable()
    {
        inputActions.Player.Shoot.performed += Shoot;
        inputActions.Player.Shoot.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Shoot.performed -= Shoot;
        inputActions.Player.Shoot.Disable();
    }

    private void Shoot(InputAction.CallbackContext obj)
    {
        if (!IsOwner) return;

        string team = PlayerPrefs.GetString("Team");
        Vector3 spawnPosition = cameraHolder.position + cameraHolder.forward * firePointDistance;
        Quaternion spawnRotation = cameraHolder.rotation;
        GameObject audioManager = GameObject.Find("AudioManager");
        PlayerSounds playerSounds = audioManager.GetComponent<PlayerSounds>();
        playerSounds.playSound("Shoot");

        GenerateBulletServerRpc(spawnPosition, spawnRotation, team);
    }

    [ServerRpc]
    private void GenerateBulletServerRpc(Vector3 spawnPosition, Quaternion spawnRotation, string team)
    {
        GenerateBulletClientRpc(spawnPosition, spawnRotation, team);
    }

    [ClientRpc]
    private void GenerateBulletClientRpc(Vector3 spawnPosition, Quaternion spawnRotation, string team)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        projectileObject.SetActive(false);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.InitializeTeam(team);
        projectileObject.SetActive(true);
        Debug.Log("Created a Bullet! From team: " + team);
    }

}