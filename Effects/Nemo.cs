using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Nemo : IEffect
    {
        public string Name => "Nemo";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Doot Doot Fish";

        public void Use()
        {
            int NemoSpawn = UnityEngine.Random.Range(3, 5);
            if (GetEnemies.Nemo == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Nemo, NemoSpawn, false);
        }
    }
}
