using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Timer : NetworkBehaviour
{
    public float startTimeInSeconds = 600f;
    private NetworkVariable<float> currentTime = new NetworkVariable<float>(600f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public TextMeshProUGUI timerText;

    private bool timerRunning = false;
    private bool EndSquenceStarted = false;

    public GameComplete gameComplete;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartTimer();
        }
    }


    void Update()
    {
        if (IsServer)
        {
            if (timerRunning)
            {
                currentTime.Value -= Time.deltaTime;
                if (currentTime.Value <= 0)
                {
                    currentTime.Value = 0;
                    StopTimer();

                    // TODO: Add some logic about the game state


                }

                // TODO: remove this before pushing

                //if (currentTime.Value >= 10f) {
                //    currentTime.Value = 10f;
                //}
            }
        }
        UpdateTimerDisplay();
        
        // End game if at 0 time left
        if (!timerRunning && currentTime.Value <= 0 && !EndSquenceStarted) {
            EndSquenceStarted = true;
            Debug.Log("Game ended");
            gameComplete.EndGame(EndGameReason.timeout);
        }
    }

    
    public void StartTimer()
    {
        // Initialize the timer
        Debug.Log("Starting Timer");
        currentTime.Value = startTimeInSeconds;
        timerRunning = true;
    }

    
    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        currentTime.Value = startTimeInSeconds;
        UpdateTimerDisplay();
    }

    // Function to update the TextMeshProUGUI display of the timer
    void UpdateTimerDisplay()
    {
        // Format the time in minutes and seconds (MM:SS)
        string minutes = Mathf.Floor(currentTime.Value / 60).ToString("00");
        string seconds = Mathf.Floor(currentTime.Value % 60).ToString("00");

        string timeText = minutes + ":" + seconds;

        // Update TextMeshProUGUI for the Canvas HUD
        if (timerText != null)
        {
            timerText.text = timeText;
        }
    }
}
