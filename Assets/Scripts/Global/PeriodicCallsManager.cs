using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HunterZone.Space
{
    public class PeriodicCallsManager : MonoBehaviour
    {
        public static PeriodicCallsManager Instance { get => instance; }
        private static PeriodicCallsManager instance;

        public List<PeriodicCall> PeriodicCalls;

        private void Awake()
        {
            instance = this;
            PeriodicCalls = new List<PeriodicCall>();
            DontDestroyOnLoad(gameObject);
        }
        private void Update()
        {
            HandlePeriodicCalls();
        }

        private void HandlePeriodicCalls()
        {
            foreach (PeriodicCall _call in PeriodicCalls)
            {
                if (_call.active)
                {
                    if (_call.timer <= 0)
                    {
                        _call.timer = _call.period;
                        _call.Call();
                    }
                    _call.timer -= Time.deltaTime;
                }
            }
        }
    }
    public class PeriodicCall : IDisposable
    {
        public bool active;
        public float timer;
        public float period;
        public event Action onCalled;
        public PeriodicCall()
        {
            timer = 0f;
            active = false;
            PeriodicCallsManager.Instance.PeriodicCalls.Add(this);
        }
        public void Call()
        {
            onCalled?.Invoke();
        }

        public void Dispose()
        {
            foreach (Action _action in onCalled.GetInvocationList())
            {
                onCalled -= _action;
            }
            timer = 0f;
            active = false;
            PeriodicCallsManager.Instance.PeriodicCalls.Remove(this);
        }
    }
}
