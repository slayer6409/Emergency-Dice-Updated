using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InsideShrimps : IEffect
    {
        public string Name => "Inside Shrimps";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "SHRIMPS";

        public void Use()
        {
            int shrimpSpawn = UnityEngine.Random.Range(5, 10);
            if (GetEnemies.Shrimp == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Shrimp, shrimpSpawn, true);
        }
    }
}
