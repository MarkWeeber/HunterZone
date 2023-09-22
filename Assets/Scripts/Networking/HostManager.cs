using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class HostManager : MonoBehaviour
    {
        [SerializeField] private int maxPlayer = 8;
        public Lobby lobby {  get; private set; }

        private static HostManager instance;
        public static HostManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HostManager();
                }
                return instance;
            }
        }
        private float heartBeatTimer = 0f;
        private bool creatingLobby = false;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            //HandleLobbyHeartBeat();
        }

        private async void HandleLobbyHeartBeat()
        {
            if (lobby != null)
            {
                heartBeatTimer -= Time.deltaTime;
                if (heartBeatTimer <= 0f)
                {
                    await Lobbies.Instance.SendHeartbeatPingAsync(lobby.Id);
                    heartBeatTimer = 15f;
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
                            }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { "LobbyName", new DataObject(DataObject.VisibilityOptions.Public, lobbyName, DataObject.IndexOptions.S1) }
                    }
                };
                _options.IsPrivate = true;
                lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayer, _options);
                InformationPanelUI.Instance.SendInformation($"Lobby with name {lobby.Name} created successfuly!", InfoMessageType.SUCCESS);
                creatingLobby = false;
                heartBeatTimer = 15f;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogException(exception);
                InformationPanelUI.Instance.SendInformation(exception.ToString(), InfoMessageType.ERROR);
                creatingLobby = false;
            }
        }
    }
}