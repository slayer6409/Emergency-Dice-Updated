using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class SpeedUp : IEffect
    {
        public string Name => "$Speed Up Time";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Hehe Time go brrr";

        public void Use()
        {
            Networker.Instance.TimeScaleServerRPC();
        }
    }
}
