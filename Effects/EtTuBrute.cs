using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class EtTuBrute : IEffect
    {
        public string Name => "Et Tu Brute";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Et tu, Brute?";

        public void Use()
        {
            Networker.Instance.SpawnSurroundedServerRPC(GetEnemies.allEnemies.First(x=>x.enemyName=="Lord Of The Manor").enemyName, 5, 2);
        }

    }
}
