using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsHandler : MonoBehaviour
{
    private static GameSettingsHandler instance;
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
    }

    void Start()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1);
        AudioListener.volume = masterVol;
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
}
