using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Barbers : IEffect
    {
        public string Name => "Barbers";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Time for a hair cut";

        public void Use()
        {
            int barberSpawn = UnityEngine.Random.Range(2, 4);
            if (GetEnemies.Barber == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Barber, barberSpawn, true);
        }
    }
}
