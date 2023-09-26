using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterZone.Space
{
    public class LobbySingleton : MonoBehaviour
    {
        [SerializeField] private int maxConnections = 10;
        [SerializeField] private float heartBeatTime = 15f;
        [SerializeField] private float lobbyPollingTime = 1.2f;
        private static LobbySingleton instance;
        public static LobbySingleton Instance { get => instance; }
        public LobbyManager LobbyManager;
        public List<PeriodicCall> callers = new List<PeriodicCall>();

        private void Awake()
        {
            instance = this;
            LobbyManager = new LobbyManager()
            {
                MaxConncetions = maxConnections,
                HeartBeatTime = heartBeatTime,
                LobbyPollingTime = lobbyPollingTime,
                PlayerConfig = new PlayerConfig()
                {
                    Name = PlayerPrefs.GetString(GlobalStringVars.PREFS_NAME, "Unnamed Player"),
                    Color = new Color(
                                        PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_R, 0),
                                        PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_G, 1),
                                        PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_B, 0),
                                        1)
                }
            };
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            LobbyManager.Dispose();
        }
    }
}