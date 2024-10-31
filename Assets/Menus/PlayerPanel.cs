using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.Services.Authentication;

public class PlayerPanel : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text ReadyState;

    public void SetData(Player player)
    {
        string name = player.Data["PlayerName"].Value;
        if (player.Id == AuthenticationService.Instance.PlayerId)
        {
            name = "You [" + name + "]";
        }
        Name.SetText(name);
        ReadyState.SetText("");
    }

    public void ClearData()
    {
        Name.SetText("");
        ReadyState.SetText("");
    }
    
}
