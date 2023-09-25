using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HunterZone.Space
{
    public class ApplicationController : MonoBehaviour
    {
        public const int k_signInTries = 5;
        public const int signInDealyMiliseconds = 15000;

        [SerializeField] private List<GameObject> spawnPrefabs;
        private static bool initialized = false;
        private bool signInSuccess = false;

        private void Awake()
        {
            if (!initialized)
            {
                initialized = true;
                SpawPrefabs();
            }
        }

        private async void Start()
        {
            if(PlayerAuthentication.Instance != null)
            {
                string profileName = "p" + Guid.NewGuid().ToString();
                profileName = profileName.Replace("-", "");
                profileName = profileName.Substring(0, 15);
                int tries = k_signInTries;
                while (!signInSuccess)
                {
                    InformationPanelUI.Instance.SendInformation("Signing In", InfoMessageType.NOTE);
                    signInSuccess = await PlayerAuthentication.Instance.TrySignInAsync(profileName);
                    if (signInSuccess)
                    {
                        InformationPanelUI.Instance.SendInformation("Signed in!", InfoMessageType.SUCCESS);
                        break;
                    }
                    else
                    {
                        await Task.Delay(signInDealyMiliseconds);
                        tries--;
                    }
                    if (tries < 0)
                    {
                        InformationPanelUI.Instance.SendInformation("Signed in failed!", InfoMessageType.ERROR);
                        break;
                    }
                }
            }
        }

        private void SpawPrefabs()
        {
            foreach (GameObject prefab in spawnPrefabs)
            {
                GameObject spawn = Instantiate(prefab);
                DontDestroyOnLoad(spawn);
            }
        }
    }
}