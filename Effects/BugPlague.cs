using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BugPlague : IEffect
    {
        public string Name => "Bug Plague";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "A lot of bugs spawned inside!";

        public void Use()
        {
            int centipedeToSpawn = UnityEngine.Random.Range(15, 30);
            int bugToSpawn = UnityEngine.Random.Range(15, 30);
            if (GetEnemies.HoardingBug == null || GetEnemies.Centipede == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.HoardingBug, bugToSpawn, true);
            Misc.SpawnEnemyForced(GetEnemies.Centipede, centipedeToSpawn, true);
        }
    }
}
