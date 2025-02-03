using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ForceTakeoff : IEffect
    {
        public string Name => "Force Takeoff";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Forces the ship to Takeoff";
        public void Use()
        {
            StartOfRound.Instance.ShipLeaveAutomatically(false);
        }
    }
}
