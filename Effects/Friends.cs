using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Friends : IEffect
    {
        public string Name => "Friends";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Friends";

        public void Use()
        {
            int CrystalRayToSpawn = UnityEngine.Random.Range(3, 10);
            if (GetEnemies.CrystalRay == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.CrystalRay, CrystalRayToSpawn, true);
        }
    }
}
