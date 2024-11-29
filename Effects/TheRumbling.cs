using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TheRumbling : IEffect
    {
        public string Name => "The Rumbling";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "If a tree squishes you in the forest, will you make a sound?";

        public void Use()
        {
            int titanSpawn = UnityEngine.Random.Range(10, 15);
            if (GetEnemies.RedwoodTitan == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.RedwoodTitan, titanSpawn, false);
        }
    }
}
