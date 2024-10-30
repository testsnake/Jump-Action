using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using TMPro;

public class PlayerPanel : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text ReadyState;

    public void SetData(Player player)
    {
        Name.SetText(player.Data["PlayerName"].Value);
        ReadyState.SetText("");
    }
    
}
