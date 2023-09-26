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
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class LobbiesUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform lobbiesPanel;
        [SerializeField] private Transform lobbiesListContentHolder;
        [SerializeField] private Transform lobbyItemPrefab;
        [SerializeField] private TMP_Text lobbyNameTitle;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button closeLobbyButton;
        [SerializeField] private UnityEvent OnLobbyMissing;
        
        private Lobby lobby;
        private LobbyManager lobbyManager;
        private List<LobbyItem> lobbyItemsList;

        private void Start()
        {
            lobbyManager = LobbySingleton.Instance.LobbyManager;
            lobbyManager.OnLobbyClosed += OnLobbyClosed;
            lobbyManager.OnLobbyJoined += OnLobbyJoined;
            lobbyManager.OnLobbyKicked += OnLobbyKicked;
            lobbyManager.OnLobbyLeft += OnLobbyLeft;
            lobbyManager.OnLobbyPolled += OnLobbyPolled;
        }

        private void OnDestroy()
        {
            lobbyManager.OnLobbyClosed -= OnLobbyClosed;
            lobbyManager.OnLobbyJoined -= OnLobbyJoined;
            lobbyManager.OnLobbyKicked -= OnLobbyKicked;
            lobbyManager.OnLobbyLeft -= OnLobbyLeft;
            lobbyManager.OnLobbyPolled -= OnLobbyPolled;
            lobbyManager = null;
        }

        private void OnLobbyLeft()
        {
            lobby = null;
            HideLobbiesPanel();
        }

        private void OnLobbyJoined(Lobby lobby)
        {
            this.lobby = lobby;
            lobbyNameTitle.text = lobby.Name;
            ShowLobbiesPanel();
        }

        private void OnLobbyClosed()
        {
            lobby = null;
            HideLobbiesPanel();
        }

        private void OnLobbyKicked()
        {
            lobby = null;
            HideLobbiesPanel();
        }

        private void OnLobbyPolled(Lobby lobby)
        {
            UpdatePlayersList();
        }

        private void UpdatePlayersList()
        {
            lobbyItemsList = new List<LobbyItem>(lobbiesListContentHolder.GetComponentsInChildren<LobbyItem>().ToList());
            foreach (Player _player in lobby.Players)
            {
                LobbyItem _item = lobbyItemsList.Find(x => x.PlayerId == _player.Id);
                if (_item == null) // adding new player lobby items
                {
                    Transform spawnedPrefab = Instantiate(lobbyItemPrefab, lobbiesListContentHolder.transform);
                    LobbyItem _lobbyItem = spawnedPrefab.GetComponent<LobbyItem>();
                    _lobbyItem.PlayerId = _player.Id;
                    TMP_Text _text = spawnedPrefab.GetComponentInChildren<TMP_Text>();
                    if (_text != null)
                    {
                        _text.text = _player.Data["PlayerName"].Value;
                    }
                }
            }
            foreach (LobbyItem _item in lobbyItemsList)
            {
                Player _player = lobby.Players.Find(x => x.Id == _item.PlayerId);
                if (_player == null) // clearing of non existent lobby items
                {
                    Destroy(_item.gameObject);
                }
            }
        }

        private void ClearPlayersList()
        {
            lobbyItemsList = lobbiesListContentHolder.GetComponentsInChildren<LobbyItem>().ToList();
            if (lobbyItemsList != null)
            {
                foreach (LobbyItem _item in lobbyItemsList)
                {
                    Destroy(_item.gameObject);
                }
            }
        }

        private void HideLobbiesPanel()
        {
            ClearPlayersList();
            lobbiesPanel.gameObject.SetActive(false);
        }

        private void ShowLobbiesPanel()
        {
            UpdatePlayersList();
            lobbiesPanel.gameObject.SetActive(true);
        }

        public async void LeaveLobbyClick(CallBackUI callBackUI = null)
        {
            await lobbyManager.LeaveJoinedLobbyAsync();
            if (callBackUI != null)
            {
                callBackUI.Actions?.Invoke();
            }
        }

        public async void CloseLobbyClick(CallBackUI callBackUI = null)
        {
            await lobbyManager.CloseJoinedLobbyAsync();
            if (callBackUI != null)
            {
                callBackUI.Actions?.Invoke();
            }
        }

        public async void StartGameClick(CallBackUI callBackUI = null)
        {
            Debug.Log("Start game click");
            await Task.Delay(1000);
            if (callBackUI != null)
            {
                callBackUI.Actions?.Invoke();
            }
        }
    }
}