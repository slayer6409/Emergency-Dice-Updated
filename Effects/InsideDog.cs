using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InsideDog : IEffect
    {
        public string Name => "Inside Dog";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Can I pet that Dawg";

        public void Use()
        {
            if (GetEnemies.Dog == null)
                return;
            Networker.Instance.PlaySoundServerRPC("Dawg");
            Misc.SpawnEnemyForced(GetEnemies.Dog, 1, true);
        }
    }
}
