using UnityEngine;

public class TeamScoreManager : MonoBehaviour
{
    [Header("Team Scores")]
    [SerializeField] private int blueTeamScore = 0; // Blue team's score
    [SerializeField] private int redTeamScore = 0;  // Red team's score
    [SerializeField] private readonly int scoreToWin = 5; // Set to 5, settings to 1 for development

    // Event to notify when the score is updated (for UI updates)
    public delegate void OnScoreChanged(int blueScore, int redScore);
    public event OnScoreChanged ScoreChanged;

    // Blue is 0, red is 1, add more if more teams needed
    public delegate void OnTeamWin(int teamNumber, int blueScore, int redScore);
    public event OnTeamWin TeamWins;

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
        // Do weird check to allow for an "overtime" on edge case both teams score final point at same time
        if ((blueTeamScore >= scoreToWin && !(redTeamScore >= blueTeamScore)) || (redTeamScore >= scoreToWin && !(blueTeamScore >= redTeamScore))) {
            NotifyTeamWins();
        }
        
    }

    private void NotifyTeamWins() {
        int team = -1;
        if (blueTeamScore == scoreToWin && blueTeamScore > redTeamScore) {
            team = 0;
        } else if (redTeamScore == scoreToWin && redTeamScore > blueTeamScore) {
            team = 1;
        }
        Debug.Log("TEAM " + team + " WINS");
        TeamWins?.Invoke(team, blueTeamScore, redTeamScore);

    }

    // Future networking methods can go here
    public virtual void SyncScores()
    {
        // Placeholder for future networking implementation
        Debug.Log("SyncScores called - implement this in a derived class or network system.");
    }
}
