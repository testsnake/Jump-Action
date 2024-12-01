using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{
    public AudioSource music;

    void Start()
    {
        music.Play();
    }

    public void Update()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1);
        music.volume = musicVol;
    }
}
