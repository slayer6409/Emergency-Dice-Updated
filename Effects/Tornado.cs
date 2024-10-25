using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Tornado : IEffect
    {
        public string Name => "Tornado";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "It Spin";

        public void Use()
        {
            int TornadoSpawn = UnityEngine.Random.Range(1, 3);
            if (GetEnemies.Tornado == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Tornado, TornadoSpawn, false);
        }
    }
}
