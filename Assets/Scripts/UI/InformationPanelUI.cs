using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class InformationPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform informationContainer;
        [Header("Settings")]
        [SerializeField] private bool showConsoleMessages = false;
        [SerializeField] private float fadeOutTime = 3f;
        [SerializeField] private Color noteColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color errorColor;
        [SerializeField] private Color successColor;

        private static InformationPanelUI instance;
        private List<TMP_Text> textList;
        private int index = -1;
        private int count = 0;
        public static InformationPanelUI Instance => instance;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            textList = informationContainer.GetComponentsInChildren<TMP_Text>().ToList();
            count = textList.Count;
            if (showConsoleMessages)
            {
                Application.logMessageReceived += HandleConsoleMessages;
            }
        }

        private void OnDestroy()
        {
            if (showConsoleMessages)
            {
                Application.logMessageReceived -= HandleConsoleMessages;
            }
        }

        private void Update()
        {
            HandleInformationMessageShow();
        }

        private void HandleInformationMessageShow()
        {
            foreach (TMP_Text _text in textList)
            {
                float alpha = _text.color.a;
                float timer = alpha * fadeOutTime;
                if (alpha > 0.005f)
                {
                    timer -= Time.deltaTime;
                    _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Lerp(0f, 1f, Mathf.Abs(timer / fadeOutTime)));
                }
            }
            TMP_Text[] sortedArray = textList.OrderByDescending(t => t.color.a).ToArray(); // sorting
            for (int i = 0; i < sortedArray.Length; i++)
            {
                sortedArray[i].transform.SetSiblingIndex(i);
            }
        }

        public void SendInformation(string message, InfoMessageType infoMessageType = InfoMessageType.NOTE)
        {
            index++;
            if (index >= count)
            {
                index = 0;
            }
            textList[index].text = message;
            switch (infoMessageType)
            {
                case InfoMessageType.NOTE:
                    textList[index].color = noteColor;
                    break;
                case InfoMessageType.WARNING:
                    textList[index].color = warningColor;
                    break;
                case InfoMessageType.ERROR:
                    textList[index].color = errorColor;
                    break;
                case InfoMessageType.SUCCESS:
                    textList[index].color = successColor;
                    break;
                default:
                    break;
            }
        }

        private void HandleConsoleMessages(string condition, string stackTrace, LogType type)
        {
            SendInformation(condition + " + " + stackTrace, InfoMessageType.NOTE);
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