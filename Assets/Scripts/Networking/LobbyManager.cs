using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class LobbyManager : IDisposable
    {
        public int MaxConncetions { get => maxConnections; set => maxConnections = value; }
        private int maxConnections;
        public Lobby JoinedLobby { get; private set; }
        public event Action OnLobbyClosed;
        public event Action<Lobby> OnLobbyJoined;
        public event Action OnLobbyLeft;
        public event Action OnLobbyKicked;
        public event Action<Lobby> OnLobbyPolled;
        public float HeartBeatTime { get => heartBeatTime; set => heartBeatTime = value; }
        private float heartBeatTime;
        public float LobbyPollingTime { get => lobbyPollingTime; set => lobbyPollingTime = value; }
        private float lobbyPollingTime;
        private bool creatingLobby = false;
        private bool closingLobby = false;
        private bool kickingPlayer = false;
        private bool joining = false;
        private bool leavingLobby = false;
        private PeriodicCall heartBeatCall;
        private PeriodicCall lobbyPollCall;
        public PlayerConfig PlayerConfig { get => playerConfig; set => playerConfig = value; }
        private PlayerConfig playerConfig;

        public async void Dispose()
        {
            Debug.Log("DISPOSE CALLED");
            if (IsLobbyHost())
            {
                await CloseJoinedLobbyAsync();
                Debug.Log("LOBBY MANAGER CLOSED LOBBY ON DISPOSE");
            }
            else
            {
                await LeaveJoinedLobbyAsync();
                Debug.Log("LOBBY MANAGER LEFT LOBBY ON DISPOSE");
            }
        }

        public bool IsLobbyHost()
        {
            return JoinedLobby != null && JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        public bool IsPlayerInLobby()
        {
            if (JoinedLobby != null && JoinedLobby.Players != null)
            {
                foreach (Player player in JoinedLobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> CreateLobbyAsync(string lobbyName)
        {
            if (creatingLobby)
            {
                return false;
            }
            creatingLobby = true;
            try
            {
                CreateLobbyOptions _options = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {
                                "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, LobbySingleton.Instance.LobbyManager.PlayerConfig.Name)
                            },
                            {
                                "AuthId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)
                            }
                        }
                    }
                };
                JoinedLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxConnections, _options);
                CreateLobbyHeartBeat();
                CreateLobbyPolling();
                OnLobbyJoined?.Invoke(JoinedLobby);
                InformationPanelUI.Instance?.SendInformation($"Lobby with name {JoinedLobby.Name} created successfuly!", InfoMessageType.SUCCESS);
                creatingLobby = false;
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogException(exception);
                InformationPanelUI.Instance?.SendInformation("Lobby create error", InfoMessageType.ERROR);
                creatingLobby = false;
                return false;
            }
        }

        public async Task<bool> JoinLobbyByNameAsync(string lobbyName)
        {
            if (joining)
            {
                return false;
            }
            joining = true;
            try
            {
                QueryLobbiesOptions _lobbyQueryOptions = new QueryLobbiesOptions();
                _lobbyQueryOptions.Count = 25;
                _lobbyQueryOptions.Filters = new List<QueryFilter>()
                {
                    new QueryFilter
                    (
                        field: QueryFilter.FieldOptions.Name,
                        op: QueryFilter.OpOptions.CONTAINS,
                        value: lobbyName
                    )
                };
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(_lobbyQueryOptions);
                if (queryResponse.Results.Count > 0)
                {
                    JoinedLobby = queryResponse.Results[0];
                    JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                    if (JoinedLobby == null)
                    {
                        InformationPanelUI.Instance?.SendInformation($"No lobby with name {lobbyName} found!", InfoMessageType.WARNING);
                        joining = false;
                        return false;
                    }
                    JoinLobbyByIdOptions _joinLobbyByIdOptions = new JoinLobbyByIdOptions()
                    {
                        Player = new Player
                        {
                            Data = new Dictionary<string, PlayerDataObject>
                            {
                                {
                                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, LobbySingleton.Instance.LobbyManager.PlayerConfig.Name)
                                },
                                {
                                    "AuthId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)
                                }
                            }
                        }
                    };
                    JoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(JoinedLobby.Id, _joinLobbyByIdOptions); // joining lobby
                    if (JoinedLobby == null)
                    {
                        joining = false;
                        return false;
                    }
                    CreateLobbyPolling();
                    LobbySingleton.Instance.callers.Add(lobbyPollCall);
                    OnLobbyJoined?.Invoke(JoinedLobby);
                    InformationPanelUI.Instance?.SendInformation($"Joined successfuly to {lobbyName} lobby", InfoMessageType.SUCCESS);
                    joining = false;
                    return true;
                }
                else
                {
                    InformationPanelUI.Instance?.SendInformation($"No lobby with name {lobbyName} found!", InfoMessageType.WARNING);
                    joining = false;
                    return false;
                }
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogException(exception);
                InformationPanelUI.Instance?.SendInformation("Lobby service error", InfoMessageType.ERROR);
                joining = false;
                return false;
            }
        }

        public async Task<bool> CloseJoinedLobbyAsync()
        {
            if (closingLobby)
            {
                return false;
            }
            if (JoinedLobby == null)
            {
                closingLobby = false;
                return false;
            }
            closingLobby = true;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);
                heartBeatCall.Dispose();
                lobbyPollCall.Dispose();
                OnLobbyClosed?.Invoke();
                InformationPanelUI.Instance?.SendInformation("Lobby closed", InfoMessageType.NOTE);
                JoinedLobby = null;
                closingLobby = false;
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.Log(exception);
                InformationPanelUI.Instance?.SendInformation("Lobby service error", InfoMessageType.ERROR);
                closingLobby = false;
                return false;
            }
        }

        public async Task<bool> LeaveJoinedLobbyAsync()
        {
            if (leavingLobby)
            {
                return false;
            }
            if (JoinedLobby == null)
            {
                leavingLobby = false;
                return false;
            }
            leavingLobby = true;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
                heartBeatCall.Dispose();
                lobbyPollCall.Dispose();
                OnLobbyLeft?.Invoke();
                InformationPanelUI.Instance?.SendInformation("Lobby left", InfoMessageType.NOTE);
                JoinedLobby = null;
                leavingLobby = false;
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.Log(exception);
                InformationPanelUI.Instance?.SendInformation("Lobby service error", InfoMessageType.ERROR);
                leavingLobby = false;
                return false;
            }
        }

        private void CreateLobbyHeartBeat()
        {
            heartBeatCall = new PeriodicCall();
            heartBeatCall.active = true;
            heartBeatCall.period = heartBeatTime;
            heartBeatCall.onCalled += HandleLobbyHeartBeat;
        }

        private void CreateLobbyPolling()
        {
            lobbyPollCall = new PeriodicCall();
            lobbyPollCall.active = true;
            lobbyPollCall.period = lobbyPollingTime;
            lobbyPollCall.onCalled += HandleLobbyPolling;
        }

        private async void HandleLobbyHeartBeat()
        {
            if (IsLobbyHost())
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
            }
        }

        private async void HandleLobbyPolling()
        {
            if (JoinedLobby!= null)
            {
                JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                if (!IsPlayerInLobby())
                {
                    OnLobbyKicked?.Invoke();
                    Debug.Log($"Kicked from lobby {JoinedLobby.Name}");
                    JoinedLobby = null;
                }
                OnLobbyPolled?.Invoke(JoinedLobby);
            }
        }

        public async Task<bool> KickPlayerFromLobby(Player kickPlayer)
        {
            if (kickingPlayer)
            {
                return false;
            }
            if (JoinedLobby == null)
            {
                kickingPlayer = false;
                return false;
            }
            kickingPlayer = true;
            try
            {
                await LobbyService.Instance?.RemovePlayerAsync(JoinedLobby.Id, kickPlayer.Id);
                kickingPlayer = false;
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.Log(exception);
                InformationPanelUI.Instance?.SendInformation("Lobby service error", InfoMessageType.ERROR);
                kickingPlayer = false;
                return false;
            }
        }
    }
}