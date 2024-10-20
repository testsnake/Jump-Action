using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;

public class LobbyJoinPanel : MonoBehaviour
{
    public LobbyData data;
    public TMP_Text Name;
    public TMP_Text Gamemode;
    public TMP_Text Playerslots;

    public void RefreshData()
    {
        Name.SetText(data.lobbyname);
        Gamemode.SetText(data.gamemode);
        Playerslots.SetText("Players: " + data.currentplayers + "/" + data.maxplayers);
    }

    public async void JoinLobby()
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByIdAsync(data.lobbyid);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
}

public struct LobbyData
{
    public string lobbyid;
    public string lobbyname;
    public string gamemode;
    public string maxplayers;
    public string currentplayers;
}