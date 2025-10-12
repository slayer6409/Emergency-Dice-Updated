using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AllFly : GalEffect
    {
        public string Name => "Everyone Can Fly!";
        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Double tap space to fly!!! (Like Minecraft!)";

        public void Use()
        {
            Networker.Instance.AllFlyServerRPC();
        }
    }
}
