using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class TeamScoreHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text blueTeamScoreText; // TMP text for Blue Team's score
    [SerializeField] private TMP_Text redTeamScoreText;  // TMP text for Red Team's score

    [Header("Score Manager")]
    [SerializeField] private TeamScoreManager teamScoreManager; // Reference to the TeamScoreManager

    private void Start()
    {
        if (teamScoreManager != null)
        {
            // Subscribe to the score change event
            teamScoreManager.ScoreChanged += UpdateScoreUI;

            // Initialize the score display
            UpdateScoreUI(teamScoreManager.GetBlueTeamScore(), teamScoreManager.GetRedTeamScore());
        }
        else
        {
            Debug.LogWarning("TeamScoreManager is not assigned in TeamScoreHUD.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        if (teamScoreManager != null)
        {
            teamScoreManager.ScoreChanged -= UpdateScoreUI;
        }
    }

    // Update the UI when the score changes
    private void UpdateScoreUI(int blueScore, int redScore)
    {
        if (blueTeamScoreText != null)
        {
            blueTeamScoreText.text = blueScore.ToString();
        }

        if (redTeamScoreText != null)
        {
            redTeamScoreText.text = redScore.ToString();
        }
    }
}
