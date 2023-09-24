using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
        private List<LobbyItem> playersList = new List<LobbyItem>();
        private LobbyItem lobbyItem;
        private float lobbyUpdateTimer = 0f;

        private void Start()
        {
            if(host)
            {
                HostManager.Instance.OnLobbyHosted += OnLobbyHosted;
                HostManager.Instance.OnLobbyClosed += OnLobbyClosed;
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
        }

        private void OnDestroy()
        {
            ClearPlayersList();
            lobby = null;
            if (host)
            {
                HostManager.Instance.OnLobbyHosted -= OnLobbyHosted;
                HostManager.Instance.OnLobbyClosed -= OnLobbyClosed;
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
                    lobbyUpdateTimer = 1.1f;
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
                Debug.Log(_player.ConnectionInfo);
                AddPlayerToList(_player);
                //if (playersList.FirstOrDefault(x => x.Player.Id == _player.Id) == null)
                //{
                //    AddPlayerToList(_player); // new player found, adding it
                //}
            }
            //foreach (LobbyItem _player in playersList.ToList()) // missing players
            //{
            //    if (lobby.Players.FirstOrDefault(x => x.Id == _player.Player.Id) == null)
            //    {
            //        RemovePlayerFromList(_player); // clearing missing player from our list
            //    }
            //}
        }

        private void AddPlayerToList(Player player)
        {
            instantiatedGameObject = Instantiate(lobbyItemPrefab, lobbiesListContentHolder.transform);
            playerNameText = instantiatedGameObject.GetComponentInChildren<TMP_Text>();
            lobbyItem = instantiatedGameObject.GetComponent<LobbyItem>();
            lobbyItem.Player = player;
            if (playerNameText != null)
            {
                playerNameText.text = player.Data["PlayerName"].Value;
            }
            playersList.Add(lobbyItem);
        }

        private void RemovePlayerFromList(LobbyItem player)
        {
            playersList.Remove(player);
            Destroy(player.gameObject);
        }

        private void ClearPlayersList()
        {
            foreach (LobbyItem _player in playersList.ToList())
            {
                Destroy(_player.gameObject);
            }
            playersList.Clear();
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

        public async void CloseLobby(CallBackUI callBackUI = null)
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

        public async void LeaveLobby(CallBackUI callBackUI = null)
        {
            bool? result = await ClientManager.Instance.LeaveLobbyAsync();
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
    }
}