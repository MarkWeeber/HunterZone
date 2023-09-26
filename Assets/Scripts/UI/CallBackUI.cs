using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HunterZone.Space
{
    public class CallBackUI : MonoBehaviour
    {
        public UnityEvent Actions;
        public UnityEvent ButtonActions;

        public void ActivateButtonActions()
        {
            ButtonActions?.Invoke();
        }

        public void SetButtonActions(CallBackUI callBackUI)
        {
            Actions = callBackUI.Actions;
        }

        public void ActivateActions()
        {
            Actions?.Invoke();
        }

        public void SetActions(CallBackUI callBackUI)
        {
            ButtonActions = callBackUI.ButtonActions;
        }
    }
}