using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSettingsHandler : MonoBehaviour
{
    private static GameSettingsHandler instance;
    public InputActionAsset inputActionAsset;
    public static GameSettingsHandler Instance {  get { return instance; } }
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject); 
        } else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
        LoadRebinds();
    }

    void Start()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1);
        AudioListener.volume = masterVol;
        int screenMode = PlayerPrefs.GetInt("ScreenMode", 1);
        setDisplayMode(screenMode);
        int screenRes = PlayerPrefs.GetInt("ScreenRes", 0);
        setGameResolution(screenRes);
        float mouseSens = PlayerPrefs.GetFloat("MouseSens", 0.5f);
        setMouseSensitivity(mouseSens);
    }

    public void setMasterVolume(float vol)
    {
        PlayerPrefs.SetFloat("MasterVolume", vol);
        AudioListener.volume = vol;
    }

    public void setMusicVolume(float vol)
    {
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }

    public void setSFXVolume(float vol)
    {
        PlayerPrefs.SetFloat("SFXVolume", vol);
    }

    public void setDisplayMode(int mode)
    {
        PlayerPrefs.SetInt("ScreenMode", mode);
        FullScreenMode fsm;
        switch (mode)
        {
            case 0:
                fsm = FullScreenMode.ExclusiveFullScreen; break;
            case 1:
                fsm = FullScreenMode.FullScreenWindow; break;
            case 2:
                fsm = FullScreenMode.Windowed; break;
            default:
                fsm = FullScreenMode.FullScreenWindow; break;
        }
        Screen.fullScreenMode = fsm;
    }

    public void setGameResolution(int resolution)
    {
        PlayerPrefs.SetInt("ScreenRes", resolution);
        FullScreenMode currentScreenMode = Screen.fullScreenMode;
        switch (resolution)
        {
            case 0:
                Screen.SetResolution(1920, 1080, currentScreenMode); break;
            case 1:
                Screen.SetResolution(1366, 768, currentScreenMode); break;
            case 2:
                Screen.SetResolution(1536, 864, currentScreenMode); break;
            case 3:
                Screen.SetResolution(1280, 720, currentScreenMode); break;
            case 4:
                Screen.SetResolution(1440, 900, currentScreenMode); break;
            case 5:
                Screen.SetResolution(1600, 900, currentScreenMode); break;
            default:
                Screen.SetResolution(1920, 1080, currentScreenMode); break;
        }
    }

    public void setMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSens", sensitivity);
    }

    public void LoadRebinds()
    {
        if (PlayerPrefs.HasKey("Rebinds"))
        {
            Debug.Log("Loading Rebinds");
            string rebinds = PlayerPrefs.GetString("Rebinds");
            inputActionAsset.LoadBindingOverridesFromJson(rebinds);
        }
    }
}
