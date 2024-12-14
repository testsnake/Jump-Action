using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public void setMode(string mode)
    {
        PlayerPrefs.SetString("Mode", mode); // Set Game Mode (Online / Offline)
    }
}
