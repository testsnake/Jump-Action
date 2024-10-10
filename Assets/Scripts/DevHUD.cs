using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevHUD : MonoBehaviour
{
    public TMP_Text speed;
    public TMP_Text state;
    public PlayerController player;

    void Update()
    {
        speed.SetText("Speed: " + player.rb.velocity.magnitude.ToString("F2"));
        state.SetText(player.state.ToString());
    }
}
