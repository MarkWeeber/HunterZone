using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace HunterZone.Space
{
    public class PlayerAuthentication : MonoBehaviour
    {
        private static PlayerAuthentication instance;
        public static PlayerAuthentication Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerAuthentication();
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += HandleAuthenticated;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void HandleAuthenticated()
        {
            InformationPanelUI.Instance.SendInformation("Signed In Successfuly", InfoMessageType.NOTE);
        }

        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= HandleAuthenticated;
        }
    }
}