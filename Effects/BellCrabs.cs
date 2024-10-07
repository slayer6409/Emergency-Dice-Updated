using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BellCrabs : IEffect
    {
        public string Name => "Bell Crabs";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "You can ring my bell!";

        public void Use()
        {
            int BellCrabsSpawn = UnityEngine.Random.Range(2, 4);
            if (GetEnemies.BellCrab == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.BellCrab, BellCrabsSpawn, true);
        }
    }
}
