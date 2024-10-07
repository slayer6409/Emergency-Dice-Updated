using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class MantisShrimps : IEffect
    {
        public string Name => "Mantis Shrimps";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "They can punch the sun!";

        public void Use()
        {
            int shrimpSpawn = UnityEngine.Random.Range(2, 4);
            if (GetEnemies.MantisShrimp == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.MantisShrimp, shrimpSpawn, true);
        }
    }
}
