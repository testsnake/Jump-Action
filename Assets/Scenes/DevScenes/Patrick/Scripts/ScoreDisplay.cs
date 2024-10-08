using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public TeamScore teamScore;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if (teamScore == null)
        {
            Debug.LogWarning("TeamScore reference is missing!");
        }

        UpdateScoreDisplay();
    }

    void Update()
    {
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null && teamScore != null)
        {
            scoreText.text = "Score: " + teamScore.GetScore().ToString();
        }
    }
}
