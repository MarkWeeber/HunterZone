using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class ClientManager : MonoBehaviour
    {
        private static ClientManager instance;
        public static ClientManager Instance => instance;
        public PlayerConfig PlayerConfig { get; set; }
        public Lobby Lobby { get; private set; }
        public event Action OnLobbyJoined;
        public event Action OnLobbyLeft;

        private bool joining = false;
        private bool leavingLobby = false;

        private void Awake()
        {
            instance = this;
            PlayerConfig = new PlayerConfig();
            PlayerConfig.Name = PlayerPrefs.GetString(GlobalStringVars.PREFS_NAME, "Unnamed Player");
            PlayerConfig.Color = new Color(
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_R, 0),
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_G, 1),
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_B, 0),
                1);
            DontDestroyOnLoad(gameObject);
        }

        private async void OnDestroy()
        {
            Debug.Log("CLIENT ON DESTROY");
            if (Lobby != null)
            {
//#pragma warning disable 4014
                await ForceLeave();
//#pragma warning restore 4014
            }
            Debug.Log("CLIENT ON DESTROY END");
        }

        private async Task ForceLeave()
        {
            //await ParallelWait();
            //await LeaveLobbyAsync();
            await Task.WhenAll(ParallelWait(), LeaveLobbyAsync());
        }

        private async Task ParallelWait()
        {
            await Task.Delay(500);
            Debug.Log("WAIT OVER");
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
                    Lobby = queryResponse.Results[0];
                    await Task.Delay(100);
                    Lobby = await LobbyService.Instance.GetLobbyAsync(Lobby.Id);
                    await Task.Delay(100);
                    if (Lobby == null)
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
                                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.PlayerConfig.Name)
                                },
                                {
                                    "AuthId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)
                                }
                            }
                        }
                    };
                    Lobby = await Lobbies.Instance.JoinLobbyByIdAsync(Lobby.Id, _joinLobbyByIdOptions); // joining lobby
                    await Task.Delay(200);
                    if (Lobby == null)
                    {
                        joining = false;
                        return false;
                    }
                    OnLobbyJoined?.Invoke();
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

        public async Task<bool> LeaveLobbyAsync()
        {
            if (leavingLobby || Lobby == null)
            {
                return false;
            }
            leavingLobby = true;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, AuthenticationService.Instance.PlayerId);
                OnLobbyLeft?.Invoke();
                InformationPanelUI.Instance?.SendInformation("Lobby left", InfoMessageType.NOTE);
                Lobby = null;
                leavingLobby = false;
                Debug.Log("LOBBY LEAVE SUCCESS");
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
    }
}