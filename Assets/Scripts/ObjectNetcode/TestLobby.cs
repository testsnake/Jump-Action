using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Mono.Cecil.Cil;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartbeatTimer;
    public GameObject lobbiesMenu;
    public GameObject lobbyPanelPrefab;
    //Set this through settings? Would be nice to save this to a file and load dynamically.
    public string playerName = "Anonymous";
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
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    //Dynamically change this later for other gamemodes if we do them.
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Capture the Chip", DataObject.IndexOptions.S1)}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            Debug.Log("Created lobby! " + lobby.Name + ", Max players:" + lobby.MaxPlayers + ", ID: " + lobby.Id + ", Code: " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    //Might go unused. Oh well.
    public async void UpdateLobbyGameMode(string gameMode) {
        try
        {
            Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1)}
                }
            });
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
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    //Change this to filter for other gamemodes (of which we have none.
                    new QueryFilter(QueryFilter.FieldOptions.S1, "Capture the Chip", QueryFilter.OpOptions.EQ)
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
                    gamemode = lobby.Data["GameMode"].Value,
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

    //Useful if we want to have a "join by code" feature, which we probably should eventually
    public async void JoinLobbyByCode(string code)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log("Joined Lobby with code " + code);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    //Do we want this? Maybe. It's easy as hell so might as well include it.
    public async void QuickJoin()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
        
    }

    public Player GetPlayer()
    {
        //Could store loadout data in here? Maybe?
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
        return player;
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
}
