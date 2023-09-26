using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace HunterZone.Space
{
    public class LobbyItem : MonoBehaviour
    {
        public Player Player { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
    }
}