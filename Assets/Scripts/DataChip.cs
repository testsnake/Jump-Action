using UnityEngine;

public class DataChip : MonoBehaviour
{
    [Header("DataChip Settings")]
    public Transform startingPosition; // The data chip's original position (reset point)
    public float resetHeightOffset = 2.0f; // Offset above the starting point
    private bool isBeingCarried = false; // DataChip status
    public string team;
    private PlayerSounds audioPlayer;

    void Start()
    {
        audioPlayer = GameObject.Find("AudioManager").GetComponent<PlayerSounds>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBeingCarried) return;
        if (!other.CompareTag("Player")) return;

        GameObject otherObject = other.gameObject;
        GameObject player = otherObject.transform.parent.gameObject;

        Health playerHealth = player.GetComponent<Health>();
        string playerTeam = playerHealth.GetTeam();
        if (team == playerTeam) return;

        isBeingCarried = true;

        transform.SetParent(other.transform);
        transform.position = player.transform.position + new Vector3(0, 1.5f, 0);
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        audioPlayer.playSound("Grab Chip");
    }

    public void ResetDataChip()
    {
        isBeingCarried = false;
        string dataChipParentObjectName = team + "DataChipSpawn";
        Debug.Log("dataChipParentObjectName: " +  dataChipParentObjectName);
        Transform dataChipSpawn = GameObject.Find(dataChipParentObjectName).GetComponent<Transform>();
        transform.SetParent(dataChipSpawn);

        transform.position = startingPosition.position + new Vector3(0, resetHeightOffset, 0);
        transform.localScale = Vector3.one;

        Debug.Log($"DataChip reset above the starting position by {resetHeightOffset} units.");
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}
