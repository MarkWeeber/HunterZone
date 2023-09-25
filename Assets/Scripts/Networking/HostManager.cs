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
    public class HostManager : MonoBehaviour
    {
        [SerializeField] private int maxPlayer = 8;
        public Lobby Lobby {  get; private set; }

        public event Action OnLobbyHosted;
        public event Action OnLobbyClosed;

        private static HostManager instance;
        public static HostManager Instance => instance;

        private float heartBeatTimer = 0f;
        private bool creatingLobby = false;
        private bool closingLobby = false;
        private bool kickingPlayer = false;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void OnDestroy()
        {
            await CloseLobbyAsync();
        }

        private void Update()
        {
            HandleLobbyHeartBeat();
        }

        private async void HandleLobbyHeartBeat()
        {
            if (Lobby != null)
            {
                heartBeatTimer -= Time.deltaTime;
                if (heartBeatTimer <= 0f)
                {
                    heartBeatTimer = 15f;
                    await LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
                }
            }
        }

        public async Task CreateLobbyAsync(string lobbyName)
        {
            if (creatingLobby)
            {
                return;
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
                                "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.PlayerConfig.Name)
                            },
                            {
                                "AuthId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)
                            }
                        }
                    }
                };
                Lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayer, _options);
                OnLobbyHosted?.Invoke();
                InformationPanelUI.Instance?.SendInformation($"Lobby with name {Lobby.Name} created successfuly!", InfoMessageType.SUCCESS);
                creatingLobby = false;
                heartBeatTimer = 15f;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogException(exception);
                InformationPanelUI.Instance?.SendInformation(exception.ToString(), InfoMessageType.ERROR);
                creatingLobby = false;
            }
        }

        public async Task<bool?> CloseLobbyAsync()
        {
            if (closingLobby || Lobby == null)
            {
                return null;
            }
            closingLobby = true;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
                OnLobbyClosed?.Invoke();
                InformationPanelUI.Instance?.SendInformation("Lobby closed", InfoMessageType.NOTE);
                Lobby = null;
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

        public async Task<bool?> KickPlayerFromLobby(Player kickPlayer)
        {
            if (kickingPlayer || Lobby == null)
            {
                return null;
            }
            kickingPlayer = true;
            try
            {
                await LobbyService.Instance?.RemovePlayerAsync(Lobby.Id, kickPlayer.Id);
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