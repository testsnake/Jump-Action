using UnityEngine;

public class TeamSpawn : MonoBehaviour
{
    [Header("Team Settings")]
    public TeamScoreManager teamScoreManager; // Reference to the shared TeamScoreManager
    public bool isBlueTeam; // Identify if this spawn point is for the blue team
    public string dataChipTag = "DataChip"; // Tag for the DataChip
    public string playerTag = "Player"; // Tag for the Player

    private void OnTriggerEnter(Collider other)
    {
        // If the object doesn't have the player tag, ignore it
        if (!other.CompareTag(playerTag)) return;

        // Check if the player is carrying the DataChip
        DataChip dataChip = other.GetComponentInChildren<DataChip>();
        if (dataChip == null || !dataChip.IsBeingCarried()) return;

        // If no TeamScoreManager is assigned, log a warning and return
        if (teamScoreManager == null)
        {
            Debug.LogWarning("TeamScoreManager is not assigned to the TeamSpawn.");
            return;
        }

        // Increment the team's score
        if (isBlueTeam)
        {
            teamScoreManager.IncrementBlueTeamScore();
        }
        else
        {
            teamScoreManager.IncrementRedTeamScore();
        }

        // Reset the DataChip to its starting position
        dataChip.ResetDataChip();
    }
}
