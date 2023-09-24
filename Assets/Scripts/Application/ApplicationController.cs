using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterZone.Space
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> spawnPrefabs;
        private static bool initialized = false;

        private void Awake()
        {
            if (!initialized)
            {
                initialized = true;
                SpawPrefabs();
            }
        }

        private async void Start()
        {
            if(PlayerAuthentication.Instance != null)
            {
                await PlayerAuthentication.Instance.Initialize();
            }
        }

        private void SpawPrefabs()
        {
            foreach (GameObject prefab in spawnPrefabs)
            {
                GameObject spawn = Instantiate(prefab);
                DontDestroyOnLoad(spawn);
            }
        }
    }
}