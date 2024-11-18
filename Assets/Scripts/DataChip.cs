using UnityEngine;

public class DataChip : MonoBehaviour
{
    [Header("DataChip Settings")]
    public Transform startingPosition; // The data chip's original position (reset point)
    public float resetHeightOffset = 2.0f; // Offset above the starting point
    private bool isBeingCarried = false; // DataChip status

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collides with the data chip
        if (!isBeingCarried && other.CompareTag("Player"))
        {
            isBeingCarried = true;

            // Attach the entire DataChip (parent) to the player
            transform.SetParent(other.transform);

            // Optionally adjust position relative to the player
            transform.localPosition = new Vector3(0, 1, 0); // Adjust based on where the chip should appear
        }
    }

    public void ResetDataChip()
    {
        // Detach from the player and reset position
        isBeingCarried = false;
        transform.SetParent(null); // Remove parent-child relationship

        // Reset position to above the starting point
        transform.position = startingPosition.position + new Vector3(0, resetHeightOffset, 0);

        Debug.Log($"DataChip reset above the starting position by {resetHeightOffset} units.");
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}
