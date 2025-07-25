using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class SpecialDetonate : IEffect
    {
        public string Name => "Special Detonate";
        public EffectType Outcome => EffectType.Penalty;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Kaboom";

        public void Use()
        {
            Networker.Instance.SpecialDetonateServerRPC();
        }
    }
    internal class GalDetonate : IEffect
    {
        public string Name => "Gal Detonate";
        public EffectType Outcome => EffectType.GalBad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Kaboom";

        public void Use()
        {
            Networker.Instance.GalDetonateServerRPC();
        }
    }
}
