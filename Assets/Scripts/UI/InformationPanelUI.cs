using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class InformationPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text text;
        [Header("Settings")]
        [SerializeField] private float fadeOutTime = 2f;
        [SerializeField] private Color noteColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color errorColor;
        [SerializeField] private Color successColor;

        private static InformationPanelUI instance;
        public static InformationPanelUI Instance
        {
            get
            {
                return instance;
            }
        }

        private string messageText;
        private float fadeOutTimer;

        private void Start()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            HandleInformationMessageShow();
        }

        private void HandleInformationMessageShow()
        {
            if (fadeOutTimer > 0)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0f, 1f, Mathf.Abs(fadeOutTimer / fadeOutTime)));
                fadeOutTimer -= Time.deltaTime;
            }
        }

        public void SendInformation(string message, InfoMessageType infoMessageType = InfoMessageType.NOTE)
        {
            messageText = message;
            text.text = messageText;
            fadeOutTimer = fadeOutTime;
            switch (infoMessageType)
            {
                case InfoMessageType.NOTE:
                    text.color = noteColor;
                    break;
                case InfoMessageType.WARNING:
                    text.color = warningColor;
                    break;
                case InfoMessageType.ERROR:
                    text.color = errorColor;
                    break;
                case InfoMessageType.SUCCESS:
                    text.color = successColor;
                    break;
                default:
                    break;
            }
        }
    }
    public enum InfoMessageType
    {
        NOTE = 1,
        WARNING = 2,
        ERROR = 3,
        SUCCESS = 4,
    }
}