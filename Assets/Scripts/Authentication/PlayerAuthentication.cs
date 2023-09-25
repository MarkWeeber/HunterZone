using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace HunterZone.Space
{
    public class PlayerAuthentication : MonoBehaviour
    {
        const int c_initTimeout = 10000;
        private static PlayerAuthentication instance;
        public static PlayerAuthentication Instance => instance;

        private NetworkManager networkManager;
        private bool signingIn = false;

        private void Awake()
        {
            instance = this;
        }

        private async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return true;
            }
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                Task task = WaitForInitialized();
                if (await Task.WhenAny(task, Task.Delay(c_initTimeout)) != task)
                {
                    return false;
                }
                return UnityServices.State == ServicesInitializationState.Initialized;
            }
            if (profileName != null)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions().SetProfile(profileName);
                await UnityServices.InitializeAsync(authProfile);
            }
            else
            {
                await UnityServices.InitializeAsync();
            }
            return UnityServices.State == ServicesInitializationState.Initialized;
            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await Task.Delay(100);
                }
            };
        }

        public async Task<bool> TrySignInAsync(string profileName = null)
        {
            if (!await TryInitServicesAsync(profileName))
            {
                return false;
            }
            if (signingIn)
            {
                Task task = WaitForSignedIn();
                if (await Task.WhenAny() != task)
                {
                    return false;
                }
                return AuthenticationService.Instance.IsSignedIn;
            }
            signingIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            signingIn = false;
            return AuthenticationService.Instance.IsSignedIn;
            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                {
                    await Task.Delay(100);
                }
            }
        }

        private void HandleDisconnect(ulong clientId)
        {
            if (clientId != 0 && clientId != networkManager.LocalClientId)
            {
                return;
            }
            if (networkManager.IsConnectedClient)
            {
                networkManager.Shutdown();
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.OnClientDisconnectCallback -= HandleDisconnect;
            }
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