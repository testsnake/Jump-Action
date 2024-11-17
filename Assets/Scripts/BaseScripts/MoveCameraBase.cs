using UnityEngine;

public class MoveCameraBase : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The target the camera should follow
    public string playerTag = "Player"; // Tag used to find the player GameObject

    protected virtual void Update()
    {
        // If no target is assigned, try to find the target
        if (target == null)
        {
            AssignTarget();
        }
    }

    protected virtual void AssignTarget()
    {
        // Finds the player object by tag
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject player in allPlayers)
        {
            if (IsValidTarget(player))
            {
                target = player.transform;
                Debug.Log($"Target assigned: {target.name}");
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"No valid target found with tag '{playerTag}'.");
        }
    }

    // A virtual method to check if the player is a valid target
    protected virtual bool IsValidTarget(GameObject player)
    {
        // By default, all players are valid
        return true;
    }
}
