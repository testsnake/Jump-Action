using System;
using Unity.VisualScripting;
using UnityEngine;

public enum EndGameReason {
    unknown = 0,
    teamWins = 1,
    timeout = 2,
    disconnect = 3,
    error = 4
}

public class TeamScoreManager : MonoBehaviour
{
    [Header("Team Scores")]
    [SerializeField] private int blueTeamScore = 0; // Blue team's score
    [SerializeField] private int redTeamScore = 0;  // Red team's score
    [SerializeField] private int scoreToWin = 3; // Set to 5, settings to 1 for development

    // Event to notify when the score is updated (for UI updates)
    public delegate void OnScoreChanged(int blueScore, int redScore);
    public event OnScoreChanged ScoreChanged;

    // Blue is 0, red is 1, add more if more teams needed
    public delegate void OnTeamWin(int teamNumber, EndGameReason reason, int blueScore, int redScore);
    public event OnTeamWin GameEnds;

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
            NotifiyGameEnds(EndGameReason.teamWins);
        }
        
    }

    private void NotifiyGameEnds(EndGameReason reason = EndGameReason.unknown) {

        int team = 0;
        if (blueTeamScore > redTeamScore) {
            team = 1;
        } else if (redTeamScore > blueTeamScore) {
            team = 2;
        }

        Debug.Log("TEAM " + team + " WINS");
        GameEnds?.Invoke(team, reason, blueTeamScore, redTeamScore);
    }

    public void EndGame(EndGameReason reason = EndGameReason.unknown) {
        NotifiyGameEnds(reason);
    }

    // Future networking methods can go here
    public virtual void SyncScores()
    {
        // Placeholder for future networking implementation
        Debug.Log("SyncScores called - implement this in a derived class or network system.");
    }
}
