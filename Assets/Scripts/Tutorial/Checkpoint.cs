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
        if (spawnPoint != null)
            spawnPoint.position = transform.position + new Vector3(0f, 1f, 0f);
        if (tutorialText != "")
            tutorialDisplay.text = tutorialText;
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
