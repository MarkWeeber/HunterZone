using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class ClientManager : MonoBehaviour
    {
        private static ClientManager instance;
        public static ClientManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClientManager();
                }
                return instance;
            }
        }
        public PlayerConfig PlayerConfig { get; set; }
        public Lobby Lobby { get; private set; }

        private bool joining = false;

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

        public async Task JoinLobbyByNameAsync(string lobbyName)
        {
            if (joining)
            {
                return;
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
                Debug.Log(queryResponse.Results.Count);
                if (queryResponse.Results.Count > 0)
                {
                    Lobby = queryResponse.Results[0];
                    JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions()
                    {
                        Player = new Player
                        {
                            Data = new Dictionary<string, PlayerDataObject>
                            {
                                {
                                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.PlayerConfig.Name)
                                }
                            }
                        }
                    };
                    //await Lobbies.Instance.JoinLobbyByIdAsync(Lobby.Id); // joining lobby
                    InformationPanelUI.Instance.SendInformation($"Lobby with name {lobbyName} found successfuly!", InfoMessageType.SUCCESS);
                }
                else
                {
                    InformationPanelUI.Instance.SendInformation($"No lobby with name {lobbyName} found!", InfoMessageType.WARNING);
                }
                joining = false;

            }
            catch (LobbyServiceException exception)
            {
                Debug.LogException(exception);
                InformationPanelUI.Instance.SendInformation(exception.ToString(), InfoMessageType.ERROR);
                joining = false;
            }
        }
    }
}