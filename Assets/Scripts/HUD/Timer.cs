using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float startTimeInSeconds = 600f;
    private float currentTime;

    public TextMeshProUGUI timerText;

    private bool timerRunning = false;

    void Start()
    {
        // Initialize the timer
        currentTime = startTimeInSeconds;
        UpdateTimerDisplay();
        StartTimer();
    }

    void Update()
    {
        if (timerRunning)
        {
            
            currentTime -= Time.deltaTime;

            
            if (currentTime <= 0)
            {
                currentTime = 0;
                StopTimer();

                // TODO: Add some logic about the game state

            }

            UpdateTimerDisplay();
        }
    }

    
    public void StartTimer()
    {
        timerRunning = true;
    }

    
    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startTimeInSeconds;
        UpdateTimerDisplay();
    }

    // Function to update the TextMeshProUGUI display of the timer
    void UpdateTimerDisplay()
    {
        // Format the time in minutes and seconds (MM:SS)
        string minutes = Mathf.Floor(currentTime / 60).ToString("00");
        string seconds = Mathf.Floor(currentTime % 60).ToString("00");

        string timeText = minutes + ":" + seconds;

        // Update TextMeshProUGUI for the Canvas HUD
        if (timerText != null)
        {
            timerText.text = timeText;
        }
    }
}
