using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    public Transform spawnPoint;
    public TMP_Text tutorialDisplay;
    public string tutorialText;
    public GameObject cheers;
    public GameObject fireworkAudio;
    public GameObject fireworks;

    private void OnTriggerEnter(Collider other)
    {
        // Update spawn point when entering checkpoint
        if (spawnPoint != null)
            spawnPoint.position = transform.position + new Vector3(0f, 1f, 0f);

        // Update tutorial guiding text when entering checkpoint
        if (tutorialText != "")
            tutorialDisplay.text = tutorialText;

        // Start tutorial end sequence
        if (tutorialText.Contains("Congratulations"))
        {
            cheers.SetActive(true);
            fireworkAudio.SetActive(true);
            fireworks.SetActive(true);
            SceneChange sceneManager = GameObject.Find("SceneManager").GetComponent<SceneChange>();
            sceneManager.LoadSceneByNameWithDelay("MainMenu", 10f);
        }
    }
}
