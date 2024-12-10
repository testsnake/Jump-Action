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
        if (!other.CompareTag(playerTag)) return;

        DataChip dataChip = other.GetComponentInChildren<DataChip>();
        if (dataChip == null || !dataChip.IsBeingCarried()) return;

        GameObject otherObject = other.gameObject;
        GameObject player = otherObject.transform.parent.gameObject;
        Health playerHealth = player.GetComponent<Health>();
        string playerTeam = playerHealth.GetTeam();
        bool isBlue = playerTeam == "Blue" ? true : false;

        if (isBlue != isBlueTeam) return;

        if (teamScoreManager == null)
        {
            Debug.LogWarning("TeamScoreManager is not assigned to the TeamSpawn.");
            return;
        }

        if (isBlueTeam)
        {
            teamScoreManager.IncrementBlueTeamScore();
        }
        else
        {
            teamScoreManager.IncrementRedTeamScore();
        }

        dataChip.ResetDataChip();
    }
}
