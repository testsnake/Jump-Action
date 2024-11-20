using UnityEngine;
using Unity.Netcode;

public class WallRunningNetcode : NetworkBehaviour
{
    private WallRunningBase wallRunningBase;

    private void Awake()
    {
        // Reference or attach the WallRunningBase component
        wallRunningBase = GetComponent<WallRunningBase>();

        if (!IsOwner)
        {
            // Disable both this script and the WallRunningBase component for non-owners
            enabled = false;
            wallRunningBase.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Optional: Add any Netcode-specific logic if needed
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Optional: Add Netcode-specific physics logic here
    }

    public void PerformWallJump()
    {
        if (!IsOwner) return;

        wallRunningBase.wallJump();
    }

    private bool IsOwnerCheck()
    {
        return GetComponent<NetworkObject>()?.IsOwner ?? false;
    }
}
