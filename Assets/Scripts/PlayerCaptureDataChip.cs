using UnityEngine;

public class PlayerCaptureDataChip : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform basePosition; // The player's base where the data chip needs to be returned

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player reaches their base while carrying the data chip
        if (other.name == "BlueTeamSpawn")
        {
            DataChip dataChip = other.GetComponent<DataChip>();
            if (dataChip != null && dataChip.IsBeingCarried())
            {
                Debug.Log("DataChip successfully returned to base!");
                dataChip.ResetDataChip(); // Reset the data chip to its original position
            }
        }
    }
}
