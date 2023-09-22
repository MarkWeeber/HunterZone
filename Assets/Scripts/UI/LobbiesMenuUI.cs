using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HunterZone.Space
{
    public class LobbiesMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField createLobbyInputField;
        [SerializeField] private TMP_InputField searchLobbyInputField;
        [Header("Settings")]
        [SerializeField] private int minLobbyNameLength = 3;
        [SerializeField] private int maxLobbyNameLength = 25;

        private void Start ()
        {
            createLobbyInputField.text = PlayerPrefs.GetString(GlobalStringVars.PREFS_CREATEDLOBBY_NAME, string.Empty);
            searchLobbyInputField.text = PlayerPrefs.GetString(GlobalStringVars.PREFS_SEARCHEDLOBBY_NAME, string.Empty);
        }

        public async void CreateLobby()
        {
            if (createLobbyInputField.text.Length >= minLobbyNameLength && createLobbyInputField.text.Length <= maxLobbyNameLength)
            {
                await HostManager.Instance.CreateLobbyAsync(createLobbyInputField.text);
                if (HostManager.Instance.lobby != null)
                {
                    PlayerPrefs.SetString(GlobalStringVars.PREFS_CREATEDLOBBY_NAME, createLobbyInputField.text);
                }
            }
            else
            {
                InformationPanelUI.Instance.SendInformation($"Lobby name must be at least {minLobbyNameLength} symbols and {maxLobbyNameLength} max ", InfoMessageType.WARNING);
            }
        }

        public async void JoinLobby()
        {
            if (searchLobbyInputField.text.Length >= minLobbyNameLength && searchLobbyInputField.text.Length <= maxLobbyNameLength)
            {
                await ClientManager.Instance.JoinLobbyByNameAsync(searchLobbyInputField.text);
                if (ClientManager.Instance.Lobby != null)
                {
                    PlayerPrefs.SetString(GlobalStringVars.PREFS_SEARCHEDLOBBY_NAME, searchLobbyInputField.text);
                }
            }
            else
            {
                InformationPanelUI.Instance.SendInformation($"Lobby name must be at least {minLobbyNameLength} symbols and {maxLobbyNameLength} max ", InfoMessageType.WARNING);
            }
        }
    }
}