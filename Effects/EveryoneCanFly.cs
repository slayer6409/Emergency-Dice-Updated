using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AllFly : IEffect
    {
        public string Name => "Everyone Can Fly!";
        public EffectType Outcome => EffectType.GalGreat;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Double tap space to fly!!! (Like Minecraft!)";

        public void Use()
        {
            Networker.Instance.AllFlyServerRPC();
        }
    }
}
