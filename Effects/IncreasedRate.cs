using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class IncreasedRate : IEffect
    {
        public string Name => "Increased Rate";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Increased company buying rate for this quota";
        public void Use()
        {
            Networker.Instance.IncreaseRateServerRPC();
        }
    }
}
