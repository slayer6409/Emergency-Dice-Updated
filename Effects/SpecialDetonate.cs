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
    internal class GalDetonate : GalEffect
    {
        public string Name => "Gal Detonate";
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Kaboom";

        public void Use()
        {
            Networker.Instance.GalDetonateServerRPC();
        }
        public EffectType RealOutcome => EffectType.Awful;
        public EffectType NoGalOutcome => EffectType.GalOnly;
    }
}
