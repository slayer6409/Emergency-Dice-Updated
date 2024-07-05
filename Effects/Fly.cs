using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Fly : IEffect
    {
        public string Name => "Fly";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Hold shift to fly!";

        public static bool CanFly = false;
        public void Use()
        {
            CanFly = true;
        }
    }
}
