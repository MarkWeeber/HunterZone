using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
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

        private NetworkManager networkManager;

        private void Awake()
        {
            instance = this;
        }

        public async Task<bool?> Initialize()
        {
            bool? result = null;
            try
            {
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.SignedIn += HandleAuthenticated;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                networkManager = NetworkManager.Singleton;
                networkManager.OnClientDisconnectCallback += HandleDisconnect;
                result = true;
            }
            catch (AuthenticationException authException)
            {
                Debug.LogWarning(authException);
                result = false;
            }
            catch (RequestFailedException requestFailException)
            {
                Debug.LogWarning(requestFailException);
                result = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception);
                result = false;
            }
            return result;

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