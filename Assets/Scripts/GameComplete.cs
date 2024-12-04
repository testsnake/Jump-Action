using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameComplete : MonoBehaviour
{

    [Header("Score Manager")]
    [SerializeField] private TeamScoreManager teamScoreManager;

    [Header("Camera")]
    [SerializeField] private Camera camera;

    private float fixedDeltaTime;


    // Start is called before the first frame update
    void Start()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
        teamScoreManager.TeamWins += EndSquence;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void EndSquence(int team, int blueTeamScore, int redTeamScore)
    {
        StartCoroutine(SlowTime());
    }

    private IEnumerator SlowTime()
    {
        yield return new WaitForSeconds(0.5f);

        float duration = 2f; // 2 seconds duration for both effects
        float elapsedTime = 0f;

        float initialTimeScale = Time.timeScale;
        float initialFOV = camera.fieldOfView;
        float targetFOV = 179;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to avoid being affected by Time.timeScale
            float t = elapsedTime / duration; // Normalize time to a value between 0 and 1

            // Exponentially decay Time.timeScale
            Time.timeScale = Mathf.Lerp(initialTimeScale, 0f, 1 - Mathf.Pow(2, -10 * t));

            // Smoothly interpolate FOV
            camera.fieldOfView = Mathf.Lerp(initialFOV, targetFOV, t);

            yield return null; // Wait for the next frame
        }

        Time.timeScale = 0f; // Ensure timeScale is exactly 0 at the end
        camera.fieldOfView = targetFOV; // Ensure FOV is exactly the target at the end
    }

}
