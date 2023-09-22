using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static Controls;

namespace HunterZone.Space
{
    [CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        public event Action<bool> FireEvent;
        public event Action<Vector2> MovementEvent;

        private Controls controls;

        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new Controls();
                controls.Player.SetCallbacks(this);
            }
            controls.Player.Enable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                FireEvent?.Invoke(true);
            }
            else if (context.canceled)
            {
                FireEvent?.Invoke(false);
            }
        }
    }
}