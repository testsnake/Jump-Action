using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class DevHUD : MonoBehaviour
{
    public TMP_Text speed;
    public TMP_Text state;
    public PlayerController player;

    void Update()
    {
        if (player == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in allPlayers)
            {
                if (p.GetComponent<NetworkObject>()?.IsOwner == true)
                {
                    player = p.GetComponent<PlayerController>();
                }
            }
        } else
        {
            speed.SetText("Speed: " + player.rb.velocity.magnitude.ToString("F2"));
            state.SetText(player.state.ToString());
        }
    }
}
