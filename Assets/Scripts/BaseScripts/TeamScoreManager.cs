using UnityEngine;

public class TeamScoreManager : MonoBehaviour
{
    [Header("Team Scores")]
    [SerializeField] private int blueTeamScore = 0; // Blue team's score
    [SerializeField] private int redTeamScore = 0;  // Red team's score

    // Event to notify when the score is updated (for UI updates)
    public delegate void OnScoreChanged(int blueScore, int redScore);
    public event OnScoreChanged ScoreChanged;

    // Increment the Blue Team's score
    public void IncrementBlueTeamScore()
    {
        blueTeamScore++;
        NotifyScoreChanged();
    }

    // Increment the Red Team's score
    public void IncrementRedTeamScore()
    {
        redTeamScore++;
        NotifyScoreChanged();
    }

    // Get the current score for the Blue Team
    public int GetBlueTeamScore()
    {
        return blueTeamScore;
    }

    // Get the current score for the Red Team
    public int GetRedTeamScore()
    {
        return redTeamScore;
    }

    // Notify listeners (e.g., UI) when the score changes
    private void NotifyScoreChanged()
    {
        ScoreChanged?.Invoke(blueTeamScore, redTeamScore);
    }

    // Future networking methods can go here
    public virtual void SyncScores()
    {
        // Placeholder for future networking implementation
        Debug.Log("SyncScores called - implement this in a derived class or network system.");
    }
}
