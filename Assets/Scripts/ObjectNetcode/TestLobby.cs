using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Threading.Tasks;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    public GameObject lobbiesMenu;
    public GameObject lobbyPanelPrefab;
    public GameObject inLobbyPanel;
    public LobbyMenu lobbyMenu;
    public MenuHandler menuHandler;
    //Set this through settings? Would be nice to save this to a file and load dynamically.
    public string playerName = "Anonymous";
    public string playerTeam = "None";
    private float heartbeatTimer;
    [SerializeField] private float heartbeatTimerMax = 15;
    public float pollTimer;
    [SerializeField] private float pollTimerMax = 1.1f;

    public const string KEY_START_GAME = "Start";
    public event EventHandler<EventArgs> OnGameStarted;

    public event EventHandler OnLeftLobby;
    public ErrorHandler errorHandler;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private void playerNameGenerator()
    {
        const string glyphs = "ABCDE0123456789";
        string prefsPlayerName = PlayerPrefs.GetString("PlayerName");
        if (!String.IsNullOrEmpty(prefsPlayerName))
        {
            playerName = prefsPlayerName;
        }
        else
        {
            playerName = "Anonymous";
            for (int i = 0; i < 5; i++)
            {
                playerName += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
            }
        }
        Debug.Log("Updating player name to: " +  playerName);
    }

    private async void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerNameGenerator();

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);
        heartbeatTimer = 0;
        pollTimer = 0;
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        if (AuthenticationService.Instance.IsSignedIn) return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        try
        {
            if (joinedLobby != null && playerTeam == "None")
            {
                int bluePlayers = 0;
                int redPlayers = 0;
                foreach (Player p in joinedLobby.Players)
                {
                    if (p.Data["PlayerTeam"].Value == "Blue")
                    {
                        bluePlayers++;
                    }
                    else if (p.Data["PlayerTeam"].Value == "Red")
                    {
                        redPlayers++;
                    }
                }
                if (bluePlayers <= redPlayers)
                {
                    UpdatePlayerTeam("Blue");
                }
                else if (redPlayers < bluePlayers)
                {
                    UpdatePlayerTeam("Red");
                }
            }
            else if (joinedLobby == null)
            {
                playerTeam = "None";
            }
            HandleLobbyHeartbeat();
            HandleLobbyPollForUpdates();
        }
        catch (Exception ex)
        {
            try
            {
                errorHandler.displayError(ex);
            } catch
            {
                Debug.LogError(ex);
            }
        }
        
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

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            pollTimer -= Time.deltaTime;
            if (pollTimer < 0)
            {
                pollTimer = pollTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                lobbyMenu.RefreshData(joinedLobby);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }
                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        TestRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                    }

                    joinedLobby = null;

                    OnGameStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    public void createLobby()
    {
        string name = playerName + "'s Lobby";
        createLobby(name, 10, "Capture the Chip");
    }

    public async void createLobby(string lobbyName = "Lobby", int maxPlayers = 10, string gameMode = "Capture the Chip")
    {
        try
        {
            //Could set lobby privacy and such. We're just gonna do all public lobbies for now.
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    //Dynamically change this later for other gamemodes if we do them.
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1)},
                    {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = lobby;
            Debug.Log("Created lobby! " + lobby.Name + ", Max players:" + lobby.MaxPlayers + ", ID: " + lobby.Id + ", Code: " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
            inLobbyPanel.SetActive(true);
            lobbyMenu.RefreshData(joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            playerTeam = "None";
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = null;
            inLobbyPanel.SetActive(false);
        }
        catch (LobbyServiceException ex)
        {
            inLobbyPanel.SetActive(false);
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    //Lobbies are deleted automatically if no players are in them, so don't worry about that.
    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    //Might go unused. Oh well.
    public async void UpdateLobbyGameMode(string gameMode) {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1)}
                }
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
            joinedLobby = lobby;
            
            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    public async void UpdatePlayerTeam(string newPlayerTeam)
    {
        try
        {
            playerTeam = newPlayerTeam;
            Debug.Log("Updating player team to: " +  playerTeam);
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerTeam", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerTeam) }
            }
            });
            joinedLobby = lobby;
            PlayerPrefs.SetString("Team", playerTeam);
            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
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
            foreach (Transform t in lobbiesMenu.transform)
            {
                Destroy(t.gameObject);
            }
            QueryResponse res = await Lobbies.Instance.QueryLobbiesAsync(query);
            Debug.Log("Lobbies found: " + res.Results.Count);
            menuHandler.switchTo(MenuHandler.menuName.lobbies);
            if (res.Results.Count > 0)
            {
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
            }
            
        } catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
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
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log("Joined Lobby with code " + code);
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            inLobbyPanel.SetActive(true);
            lobbyMenu.RefreshData(joinedLobby);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    public async Task JoinLobbyById(string id)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, options);
            Debug.Log("Joined Lobby with id " + id);
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            inLobbyPanel.SetActive(true);
            lobbyMenu.RefreshData(joinedLobby);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
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
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
        
    }

    public Player GetPlayer()
    {
        //Could store loadout data in here? Maybe?
        playerNameGenerator();
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                        {"PlayerTeam", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerTeam) }
                    }
        };
        return player;
    }

    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnQuit");
        if (joinedLobby != null)
        {
            Task.Run(async () => await LeaveOnQuit());
        }
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        if (Application.isEditor && joinedLobby != null)
        {
            Task.Run(async () => await LeaveOnQuit());
        }
    }

    private async Task LeaveOnQuit()
    {
        Debug.Log("LeaveOnQuit");
        try
        {
            playerTeam = "None";
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = null;
            inLobbyPanel.SetActive(false);
        }
        catch (LobbyServiceException ex)
        {
            try
            {
                errorHandler.displayError(ex);
            }
            catch
            {
                Debug.LogError(ex);
            }
        }
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("StartGame");

                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> {
                        {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException ex)
            {
                try
                {
                    errorHandler.displayError(ex);
                }
                catch
                {
                    Debug.LogError(ex);
                }
            }
        }
    }
}


