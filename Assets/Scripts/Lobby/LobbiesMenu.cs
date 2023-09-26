using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace HunterZone.Space
{
    public class LobbiesMenu : MonoBehaviour
    {
        [SerializeField] private GameObject lobbiesListContentHolder;
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private TMP_Text lobbyNameTitle;
        [SerializeField] private bool host = true;
        [SerializeField] private UnityEvent OnKicked = new UnityEvent();
        private Lobby lobby = null;
        public bool lobbyPersists = false;
        private GameObject instantiatedGameObject;
        private TMP_Text playerNameText;
        private LobbyItem lobbyItem;
        private float lobbyUpdateTimer = 0f;
        private LobbyEventCallbacks eventCallbacks;
        private List<LobbyItem> playersList = new List<LobbyItem>();

        private void Start()
        {
            if (host)
            {
                HostManager.Instance.OnLobbyHosted += OnLobbyHosted;
                HostManager.Instance.OnLobbyClosed += OnLobbyClosed;
                eventCallbacks = new LobbyEventCallbacks();
                eventCallbacks.PlayerJoined += HandleAddingPlayer;
                eventCallbacks.PlayerLeft += HandlePlayerLeft;
            }
            else
            {
                ClientManager.Instance.OnLobbyJoined += OnLobbyJoined;
                ClientManager.Instance.OnLobbyLeft += OnLobbyLeft;
            }
        }

        private void OnLobbyLeft()
        {
            lobby = null;
            lobbyPersists = false;
        }

        private void OnLobbyJoined()
        {
            lobby = ClientManager.Instance.Lobby;
            lobbyNameTitle.text = lobby.Name;
            lobbyPersists = true;
            ClearPreviousItems();
            UpdatePlayersList();
        }

        private void OnLobbyClosed()
        {
            lobby = null;
            lobbyPersists = false;
        }

        private void OnLobbyHosted()
        {
            lobby = HostManager.Instance.Lobby;
            lobbyNameTitle.text = lobby.Name;
            lobbyPersists = true;
            ClearPreviousItems();
            UpdatePlayersList();
        }

        private async void OnDestroy()
        {
            lobbyPersists = false;
            lobby = null;
            if (host)
            {
                HostManager.Instance.OnLobbyHosted -= OnLobbyHosted;
                HostManager.Instance.OnLobbyClosed -= OnLobbyClosed;
                eventCallbacks.PlayerJoined -= HandleAddingPlayer;
                eventCallbacks.PlayerLeft -= HandlePlayerLeft;
                await CloseLobby();
            }
            else
            {
                ClientManager.Instance.OnLobbyJoined -= OnLobbyJoined;
                ClientManager.Instance.OnLobbyLeft -= OnLobbyLeft;
            }
        }

        private void Update()
        {
            HandleLobbyUpdate();
        }

        private async void HandleLobbyUpdate()
        {
            if (lobbyPersists)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer <= 0f)
                {
                    lobbyUpdateTimer = 1.5f;
                    try
                    {
                        lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                        if (lobby != null)
                        {
                            UpdatePlayersList();
                        }
                        else
                        {
                            InformationPanelUI.Instance?.SendInformation("Lobby closed", InfoMessageType.WARNING);
                            HandleLobbyUnavailable();
                            lobbyPersists = false;
                        }
                    }
                    catch (LobbyServiceException exception)
                    {
                        InformationPanelUI.Instance?.SendInformation("Lobby closed", InfoMessageType.WARNING);
                        Debug.Log(exception);
                        HandleLobbyUnavailable();
                        lobbyPersists = false;
                    }
                }
            }
        }

        private void UpdatePlayersList()
        {
            ClearPlayersList();
            foreach (Player _player in lobby.Players) // new players
            {
                if (playersList.FirstOrDefault(x => x.Player.Id == _player.Id) == null)
                {
                    AddPlayerToList(_player); // new player found, adding it
                }
            }
            foreach (LobbyItem _player in playersList.ToList()) // missing players
            {
                if (lobby.Players.FirstOrDefault(x => x.Id == _player.Player.Id) == null)
                {
                    RemovePlayerFromList(_player); // clearing missing player from our list
                }
            }
        }

        private void AddPlayerToList(Player player)
        {
            instantiatedGameObject = Instantiate(lobbyItemPrefab, lobbiesListContentHolder.transform);
            playerNameText = instantiatedGameObject.GetComponentInChildren<TMP_Text>();
            lobbyItem = instantiatedGameObject.GetComponent<LobbyItem>();
            lobbyItem.Player = player;
            if (playerNameText != null && player != null)
            {
                playerNameText.text = player.Data["PlayerName"].Value;
            }
            playersList.Add(lobbyItem);
        }

        private void RemovePlayerFromList(LobbyItem player)
        {
            if (lobbiesListContentHolder == null)
            {
                return;
            }
            playersList.Remove(player);
            Destroy(player.gameObject);
        }

        private void ClearPlayersList()
        {
            if (lobbiesListContentHolder == null || playersList == null || !playersList.Any() || !lobbyPersists)
            {
                return;
            }
            foreach (LobbyItem _player in playersList.ToList())
            {
                try
                {
                    if (_player.gameObject != null)
                    {
                        DestroyImmediate(_player.gameObject);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                }
            }
            playersList.Clear();
        }

        private void ClearPreviousItems()
        {
            LobbyItem[] previousItems = lobbiesListContentHolder.GetComponentsInChildren<LobbyItem>();
            if (previousItems.Length > 0)
            {
                foreach (LobbyItem item in previousItems)
                {
                    Destroy(item.gameObject);
                }
            }
        }

        private void HandleLobbyUnavailable()
        {
            ClearPlayersList();
            OnKicked?.Invoke();
        }

        private async Task KickAllOtherPlayer()
        {
            foreach (Player _player in lobby.Players)
            {
                if (_player.Id == AuthenticationService.Instance.PlayerId)
                {
                    continue;
                }
                await HostManager.Instance.KickPlayerFromLobby(_player);
            }
        }

        private async void HandlePlayerLeft(List<int> leftPlayers)
        {
            Debug.Log("Player left");
            foreach (int index in leftPlayers)
            {
                await HostManager.Instance.KickPlayerFromLobby(lobby.Players[index]);
            }
        }

        private void HandleAddingPlayer(List<LobbyPlayerJoined> list)
        {
            UpdatePlayersList();
        }

        private async Task CloseLobby(CallBackUI callBackUI = null)
        {
            bool? result = await HostManager.Instance.CloseLobbyAsync();
            if (result == true)
            {
                ClearPlayersList();
                lobbyPersists = false;
                if (callBackUI != null)
                {
                    callBackUI.Actions?.Invoke();
                }
            }
            else
            {
                return;
            }
        }

        private async Task LeaveLobby(CallBackUI callBackUI = null)
        {
            bool result = await ClientManager.Instance.LeaveLobbyAsync();
            if (result == true)
            {
                ClearPlayersList();
                lobbyPersists = false;
                if (callBackUI != null)
                {
                    callBackUI.Actions?.Invoke();
                }
            }
            else
            {
                return;
            }
        }

        public async void CloseLobbyAsyncClick(CallBackUI callBackUI = null)
        {
            if (!lobbyPersists)
            {
                if (callBackUI != null)
                {
                    callBackUI.Actions?.Invoke();
                }
                return;
            }
            await CloseLobby(callBackUI);
        }
        public async void LeaveLobbyAsyncClick(CallBackUI callBackUI = null)
        {
            if (!lobbyPersists)
            {
                if (callBackUI != null)
                {
                    callBackUI.Actions?.Invoke();
                }
                return;
            }
            await LeaveLobby(callBackUI);
        }
    }
}