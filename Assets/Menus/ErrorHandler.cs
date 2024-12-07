using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorHandler : MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text data;
    public GameObject panel;

    public void displayError(Exception error)
    {
        string[] errorNameSplit = error.GetType().ToString().Split('.');
        name.text = errorNameSplit[errorNameSplit.Length - 1];
        string[] errorDataSplit = error.GetBaseException().ToString().Split(" at ");
        data.text = errorDataSplit[0];
        Debug.LogError(error.ToString());
        panel.SetActive(true);
    }

    public void closePanel()
    {
        panel.SetActive(false);
    }
}
