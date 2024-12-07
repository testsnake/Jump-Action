using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject[] allMenus;
    public TMP_Text VersionText;
    public enum menuName
    {
        lobbies = 0,
        settings = 1,
        credits = 2
    }
    public void switchToLobbies()
    {
        switchTo(menuName.lobbies);
    }
    public void switchToSettings()
    {
        switchTo(menuName.settings);
    }
    public void switchToCredits()
    {
        switchTo(menuName.credits);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void switchTo(menuName menu)
    {
        foreach (GameObject child in allMenus)
        {
            child.SetActive(false);
        }
        allMenus[(int) menu].SetActive(true);
    }

    public void Awake()
    {
        if (VersionText == null)
        { 
            VersionText = GameObject.Find("VersionText").GetComponent<TMP_Text>();
        }
    }

    public void Start()
    {
        VersionText.text = Application.version;
    }
}
