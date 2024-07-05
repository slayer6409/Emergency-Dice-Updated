using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class HealAndRestore : IEffect
    {
        public string Name => "Full restore";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Healing everyone and recharging their batteries";
        public void Use()
        {
            Networker.Instance.HealAllServerRPC();
        }
    }
}
