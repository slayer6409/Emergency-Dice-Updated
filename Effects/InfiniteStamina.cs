using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InfiniteStamina : IEffect
    {
        public string Name => "Infinite stamina";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Infinite stamina for you";
        public void Use()
        {
            PlayerControllerBPatch.HasInfiniteStamina = true;
        }
    }
}
