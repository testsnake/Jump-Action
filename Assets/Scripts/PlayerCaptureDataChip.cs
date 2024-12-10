using UnityEngine;

public class PlayerCaptureDataChip : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform basePosition; // The player's base where the data chip needs to be returned

    public void Start()
    {
        basePosition = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Health playerHealth = gameObject.GetComponent<Health>();
        string playerTeam = playerHealth.GetTeam();

        string playerTeamSpawnName = playerTeam + "TeamSpawn";
        if (other.name != playerTeamSpawnName) return;

        DataChip dataChip = other.GetComponent<DataChip>();
        if (dataChip == null) return;
        if (!dataChip.IsBeingCarried()) return;
        if (dataChip.team != playerTeam) return;

        Debug.Log("DataChip successfully returned to base!");
        dataChip.ResetDataChip(); // Reset the data chip to its original position
    }
}
