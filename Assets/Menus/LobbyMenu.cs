using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using CodeMonkey.Utils;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class LobbyMenu : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text Gamemode;
    public bool isHost;
    public Button startGameBtn;
    public GameObject[] BlueTeamSlots = new GameObject[5];
    public GameObject[] RedTeamSlots = new GameObject[5];
    private TestLobby LobbyManager;

    public void Awake()
    {
        LobbyManager = GameObject.Find("LobbyManager").GetComponent<TestLobby>();
    }


    public void RefreshData(Lobby lobby)
    {
        isHost = (lobby.HostId == AuthenticationService.Instance.PlayerId);
        Name.SetText(lobby.Name);
        Gamemode.SetText(lobby.Data["GameMode"].Value);
        int blueTeamPlayers = 0;
        int redTeamPlayers = 0;
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player: " + player.Data["PlayerName"].Value + " Team: " + player.Data["PlayerTeam"].Value);
            if (player.Data["PlayerTeam"].Value == "Blue")
            {
                BlueTeamSlots[blueTeamPlayers].GetComponent<PlayerPanel>().SetData(player);
                blueTeamPlayers++;
            }
            if (player.Data["PlayerTeam"].Value == "Red")
            {
                
                RedTeamSlots[redTeamPlayers].GetComponent<PlayerPanel>().SetData(player);
                redTeamPlayers++;
            }
        }
    }

}
