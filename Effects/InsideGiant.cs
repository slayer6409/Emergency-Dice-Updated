using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InsideGiant : IEffect
    {
        public string Name => "Inside Giant";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "A giant has been spotted inside";

        public void Use()
        {
            if (GetEnemies.Giant == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Giant, 1, true);
        }
    }
}
