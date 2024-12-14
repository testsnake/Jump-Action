using UnityEngine;
using TMPro;
using Unity.Netcode;

public class DevHUD : MonoBehaviour
{
    public TMP_Text speed;
    public TMP_Text state;
    public TMP_Text scale; 
    public PlayerControllerOffline player;

    void Update()
    {
        // Find player
        if (player == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in allPlayers)
            {
                if (p.GetComponent<NetworkObject>()?.IsOwner == true)
                {
                    player = p.GetComponent<PlayerControllerOffline>();
                }
            }
        } else
        {
            speed.SetText("Speed: " + player.rb.velocity.magnitude.ToString("F2"));
            scale.SetText("Scale: " + player.transform.localScale.y.ToString("F2"));
            state.SetText(player.state.ToString());
        }
    }
}
