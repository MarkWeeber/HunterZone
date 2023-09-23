using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterZone.Space
{
    public class InputReaderSetting : MonoBehaviour
    {
        [field: SerializeField] public InputReader InputReader {  get; private set; }
        private static InputReaderSetting instance;
        public static InputReaderSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InputReaderSetting();
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }
    }
}