using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameComplete : MonoBehaviour
{

    [Header("Score Manager")]
    [SerializeField] private TeamScoreManager teamScoreManager;

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Post Processing")]
    [SerializeField] private GlobalVolumeManager _globalVolumeManager;

    [Header("HUD")]
    [SerializeField] private GameObject _hud;
    [SerializeField] private Camera _gunHud;
    [SerializeField] private GameObject _endScreen;

    private float fixedDeltaTime;

    private CanvasGroup endScreen;


    // Start is called before the first frame update
    void Start()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
        teamScoreManager.GameEnds += EndSquence;


    }

    // Update is called once per frame
    void Update()
    {
        if (endScreen == null)
        {
            endScreen = _endScreen.GetComponent<CanvasGroup>();
            endScreen.alpha = 0;
            endScreen.interactable = false;
            endScreen.blocksRaycasts = false;
        }
    }

    public void EndGame(EndGameReason reason)
    {
        teamScoreManager.EndGame();
    }

    private void EndSquence(int team, EndGameReason reason, int blueTeamScore, int redTeamScore)
    {
        StartCoroutine(SlowTime());

    }

    private IEnumerator SlowTime()
    {
        yield return new WaitForSeconds(0.5f);

        float duration = 0.75f; // 2 seconds duration for both effects
        float elapsedTime = 0f;

        float initialTimeScale = Time.timeScale;
        float initialFOV = _camera.fieldOfView;
        float targetFOV = 150;

        float initialChromaticAberration = _globalVolumeManager.GetChromaticValue();
        _globalVolumeManager.SetDirectMode(true);

        CanvasGroup group = _hud.GetComponent<CanvasGroup>();
        float initialGunFov = _gunHud.fieldOfView;


        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to avoid being affected by Time.timeScale
            float t = HardInEaseOut(elapsedTime / duration); // Normalize time to a value between 0 and 1

            // Exponentially decay Time.timeScale
            Time.timeScale = Mathf.Lerp(initialTimeScale, 0f, 1 - Mathf.Pow(2, -10 * t));

            // Smoothly interpolate FOV
            _camera.fieldOfView = Mathf.Lerp(initialFOV, targetFOV, t);
            _globalVolumeManager.SetChromaticAberration(Mathf.Lerp(initialChromaticAberration, 1, t));
            group.alpha = Mathf.Lerp(1, 0, t);
            _gunHud.fieldOfView = Mathf.Lerp(initialGunFov, 1, t);
            endScreen.alpha = Mathf.Lerp(0, 1, t);
            yield return null; // Wait for the next frame
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        endScreen.interactable = true;
        endScreen.blocksRaycasts = true;

        Time.timeScale = 0f; // Ensure timeScale is exactly 0 at the end
        _camera.fieldOfView = targetFOV; // Ensure FOV is exactly the target at the end
    }

    float HardInEaseOut(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 0.5f); // Smoother hard-in ease-out using a sine wave
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
