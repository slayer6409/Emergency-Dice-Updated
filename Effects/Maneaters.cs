using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Maneaters : IEffect
    {
        public string Name => "Maneaters";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "A lot of maneaters spawned inside!";

        public void Use()
        {
            int maneatersToSpawn = UnityEngine.Random.Range(2, 7);
            if (GetEnemies.Maneater == null)
                return;
    
            Misc.SpawnEnemyForced(GetEnemies.Maneater, maneatersToSpawn, true);
        }

    }
}
