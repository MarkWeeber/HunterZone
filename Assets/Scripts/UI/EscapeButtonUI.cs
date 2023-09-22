using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HunterZone.Space
{
    public class EscapeButtonUI : MonoBehaviour
    {
        private Button button;
        private void Start ()
        {
            button = GetComponent<Button>();
        }

        private void Update ()
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                button.onClick.Invoke();
            }
        }
    }
}