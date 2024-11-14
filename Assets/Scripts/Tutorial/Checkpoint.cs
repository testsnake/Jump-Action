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

    private void OnTriggerEnter(Collider other)
    {
        if (spawnPoint != null)
            spawnPoint.position = transform.position;
        if (tutorialText != "")
            tutorialDisplay.text = tutorialText;
    }
}
