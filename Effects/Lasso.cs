using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Lasso : IEffect
    {
        public string Name => "Lasso";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Who are these men";

        public void Use()
        {
            int lassoSpawn = UnityEngine.Random.Range(5, 10);
            if (GetEnemies.Lasso == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Lasso, lassoSpawn, true);
        }
    }
}
