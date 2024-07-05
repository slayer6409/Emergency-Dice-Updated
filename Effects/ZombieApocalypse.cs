using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ZombieApocalypse : IEffect
    {
        public string Name => "Zombie apocalypse";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Zombie apocalypse!";
        public void Use()
        {
            int amountToSpawn = UnityEngine.Random.Range(8, 20);
            if(GetEnemies.Masked == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.Masked, amountToSpawn, true);
            
        }
    }
}
