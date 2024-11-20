using UnityEngine;
using Unity.Netcode;

public class PlayerControllerNetcode : MonoBehaviour
{
    private PlayerControllerBase playerController;

    public void Awake()
    {
        playerController = GetComponent<PlayerControllerBase>();

        if (!IsOwner)
        {
            // Disable PlayerControllerBase for non-owners
            playerController.enabled = false;
            enabled = false;
        }
        else
        {
            // Allow PlayerControllerBase logic to run
            playerController.enabled = true;
        }
    }

    public void Update()
    {
        if (!IsOwner) return;

        // Forward Update logic
        playerController.Update();
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;

        // Forward FixedUpdate logic
        playerController.FixedUpdate();
    }

    private bool IsOwner => GetComponent<NetworkObject>()?.IsOwner ?? false;
}
