using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class ColorOptionUI : MonoBehaviour
    {
        [field: SerializeField] public Color Color { get; private set; }
        [SerializeField] private Image colorPreviwer;

        private List<Button> colorOptionButtons;

        private void Start()
        {
            colorOptionButtons = GetComponentsInChildren<Button>().ToList();
            Color = LobbySingleton.Instance.LobbyManager.PlayerConfig.Color;
            colorPreviwer.color = Color;
        }

        public void PickedColor(Button button)
        {
            foreach (Button _button in colorOptionButtons)
            {
                if (_button == button)
                {
                    Color = _button.colors.normalColor;
                    colorPreviwer.color = Color;
                }
            }
        }
    }
}