using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class EscapeButtonUI : MonoBehaviour
    {
        private InputReader inputReader;
        private Button button;
        private bool signedIn = false;

        private void Start ()
        {
            inputReader = InputReaderSetting.Instance.InputReader;
            button = GetComponent<Button>();
            SignInAction();
        }

        private void OnEnable()
        {
            SignInAction();
        }

        private void OnDisable()
        {
            UnsignAction();
        }

        private void OnDestroy()
        {
            UnsignAction();
        }

        private void SignInAction()
        {
            if(!signedIn && inputReader != null)
            {
                inputReader.Escape += HandleEscape;
                signedIn = true;
            }
        }

        private void UnsignAction()
        {
            if (signedIn && inputReader != null)
            {
                inputReader.Escape -= HandleEscape;
                signedIn = false;
            }
        }

        private void HandleEscape()
        {
            if (gameObject.activeInHierarchy)
            {
                button.onClick?.Invoke();
            }
        }
    }
}