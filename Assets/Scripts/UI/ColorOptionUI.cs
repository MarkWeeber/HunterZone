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
            Color = new Color (
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_R, 0),
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_G, 1),
                PlayerPrefs.GetFloat(GlobalStringVars.PREFS_COLOR_B, 0),
                1);
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