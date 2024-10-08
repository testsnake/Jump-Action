using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevHUD : MonoBehaviour
{
    public TMP_Text speed;
    public TMP_Text state;
    public PlayerController player;

    // Update is called once per frame
    void Update()
    {
        speed.SetText("Speed: " + player.rb.velocity.magnitude.ToString("F2"));
        switch (player.state)
        {
            case PlayerController.MovementState.standing:
                state.SetText("Standing");
                break;
            case PlayerController.MovementState.crouching:
                state.SetText("Crouching");
                break;
            case PlayerController.MovementState.sliding:
                state.SetText("Sliding");
                break;
            case PlayerController.MovementState.falling:
                state.SetText("Falling");
                break;
            default:
                state.SetText("None");
                break;
        }
    }
}
