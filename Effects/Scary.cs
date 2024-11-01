using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ScaryMon : IEffect
    {
        public string Name => "Scary?";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Fear Incarnate";

        public void Use()
        {
            int ScarySpawn = UnityEngine.Random.Range(3, 12);
            if (GetEnemies.Scary == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Scary, ScarySpawn, true);
        }
    }
}
