using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuInitializer : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Dropdown screenModeDD;
    public TMP_Dropdown screenResDD;
    public Slider mouseSensSlider;
    public TMP_InputField usernameInputField;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1);
        screenModeDD.value = PlayerPrefs.GetInt("ScreenMode", 1);
        screenResDD.value = PlayerPrefs.GetInt("ScreenRes", 0);
        mouseSensSlider.value = PlayerPrefs.GetFloat("MouseSens", 0.5f);
        usernameInputField.text = PlayerPrefs.GetString("PlayerName");
    }
}
