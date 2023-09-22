using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class PlayerSettings : MonoBehaviour
    {
        [SerializeField] private ColorOptionUI ColorOptionUI;
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private int maxPlayerNameLength = 25;
        [SerializeField] private int minPlayerNameLength = 3;

        private void Start()
        {
            playerNameInputField.text = PlayerPrefs.GetString(GlobalStringVars.PREFS_NAME, string.Empty);
        }

        public void SavePlayerSettings()
        {
            if (playerNameInputField.text.Length >= minPlayerNameLength && playerNameInputField.text.Length <= maxPlayerNameLength)
            {
                if (ColorOptionUI.Color.a == 1)
                {
                    PlayerPrefs.SetString(GlobalStringVars.PREFS_NAME, playerNameInputField.text);
                    PlayerPrefs.SetFloat(GlobalStringVars.PREFS_COLOR_R, ColorOptionUI.Color.r);
                    PlayerPrefs.SetFloat(GlobalStringVars.PREFS_COLOR_G, ColorOptionUI.Color.g);
                    PlayerPrefs.SetFloat(GlobalStringVars.PREFS_COLOR_B, ColorOptionUI.Color.b);
                    InformationPanelUI.Instance.SendInformation("Save Successful!", InfoMessageType.SUCCESS);
                }
                else
                {
                    InformationPanelUI.Instance.SendInformation("Please pick color!", InfoMessageType.WARNING);
                }
            }
            else
            {
                InformationPanelUI.Instance.SendInformation($"Name must be at least {minPlayerNameLength} symbols and {maxPlayerNameLength} max ", InfoMessageType.WARNING);
            }    
        }
    }
}