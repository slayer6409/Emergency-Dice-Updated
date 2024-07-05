using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class UnlimitedStamina : IEffect
    {
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Unlimited stamina";
        public void Use()
        {
            
        }
    }
}
