using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class LobbiesMenu : MonoBehaviour
    {
        [SerializeField] private GameObject lobbiesListContentHolder;
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private TMP_Text lobbyNameTitle;
        [SerializeField] private bool host = true;
        private Lobby lobby = null;
        private GameObject instantiatedGameObject;
        private TMP_Text playerNameText;
        private List<LobbyItem> playersList = new List<LobbyItem>();
        private LobbyItem lobbyItem;
        LobbyEventCallbacks lobbyCallbacks;

        private async void OnEnable()
        {
            if (lobby == null)
            {
                if (host)
                {
                    lobby = HostManager.Instance.Lobby;
                }
                else
                {
                    lobby = ClientManager.Instance.Lobby;
                }
                if (lobby != null)
                {
                    lobbyNameTitle.text = $"{lobby.Name} lobby";
                    lobbyCallbacks = new LobbyEventCallbacks();
                    lobbyCallbacks.PlayerJoined += OnPlayerJoined;
                    lobbyCallbacks.PlayerLeft += OnPlayerLeft;
                    UpdatePlayersList();
                    await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, lobbyCallbacks);
                }
                else
                {
                    return;
                }
            }
            else
            {
                UpdatePlayersList();
            }
        }

        private void OnDestroy()
        {
            ClearPlayersList();
            lobby = null;
        }

        private void UpdatePlayersList()
        {
            if(lobby == null)
            {
                return;
            }
            ClearPlayersList();
            foreach (Player _player in lobby.Players)
            {
                AddPlayerToList(_player);
            }
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

        private void RemovePlayerFromList(Player player)
        {
            playersList.Remove(playersList.FirstOrDefault(item => item.Player == player));
        }

        private void ClearPlayersList()
        {
            Debug.Log("Clearing");
            foreach (LobbyItem _player in playersList)
            {
                Debug.Log("Destroying:" + _player.gameObject.name);
                Destroy(_player.gameObject);
            }
            playersList.Clear();
        }


        private void OnPlayerJoined(List<LobbyPlayerJoined> list)
        {
            foreach (LobbyPlayerJoined item in list)
            {
                AddPlayerToList(item.Player);
            }
        }

        private void OnPlayerLeft(List<int> list)
        {
            foreach (int item in list)
            {
                Debug.Log($"Player left:  {item}");
            }
        }

        public async void CloseLobby(CallBackUI callBackUI = null)
        {
            bool? result = await HostManager.Instance.CloseLobbyAsync();
            if (result == true)
            {
                ClearPlayersList();
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