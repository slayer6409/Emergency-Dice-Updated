using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class OutsideBugs : IEffect
    {
        public string Name => "Outside Bugs";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "A lot of bugs spawned outside!";

        public void Use()
        {
            int bugToSpawn = UnityEngine.Random.Range(10, 15);
            if (GetEnemies.HoardingBug == null || GetEnemies.Centipede == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.HoardingBug, bugToSpawn, false);
        }
    }
}
