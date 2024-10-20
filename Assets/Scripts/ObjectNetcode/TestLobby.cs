using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartbeatTimer;
    public GameObject lobbiesMenu;
    public GameObject lobbyPanelPrefab;
    [SerializeField] private float heartbeatTimerMax = 15;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void createLobby()
    {
        try
        {
            string lobbyName = "My Lobby";
            int maxPlayers = 10;
            //Could set lobby privacy and such. We're just gonna do all public lobbies for now.
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            Debug.Log("Created lobby! " + lobby.Name + ", Max players:" + lobby.MaxPlayers);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            //Can dynamically set filters through UI later probably.
            //Could consider filtering by things like gamemode, number of players, lobby name maybe? idk.
            QueryLobbiesOptions query = new QueryLobbiesOptions()
            {
                Count = 100,
                Filters = new List<QueryFilter>() { 
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>() { 
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse res = await Lobbies.Instance.QueryLobbiesAsync(query);
            Debug.Log("Lobbies found: " + res.Results.Count);
            foreach (Lobby lobby in res.Results)
            {
                Debug.Log(lobby.Name + ": " + lobby.MaxPlayers + " max players");
                GameObject lobbyPanel = GameObject.Instantiate(lobbyPanelPrefab, lobbiesMenu.transform);
                LobbyJoinPanel ljp = lobbyPanel.GetComponent<LobbyJoinPanel>();
                ljp.data = new LobbyData()
                {
                    lobbyid = lobby.Id,
                    lobbyname = lobby.Name,
                    //Dynamically change this later if we add more gamemodes
                    gamemode = "Capture the Flag",
                    currentplayers = "" + (int.Parse(lobby.MaxPlayers.ToString()) - int.Parse(lobby.AvailableSlots.ToString())),
                    maxplayers = lobby.MaxPlayers.ToString(),
                };
                ljp.RefreshData();
            }
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
    
}
