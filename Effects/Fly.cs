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
        public string Tooltip => "Double tap space to fly!!! (Like Minecraft!)";

        public static bool CanFly = false;
        
        public static float lastTapTime = -1f;
        
        public static float tapCooldown = 0.3f;
        
        public static int tapCount = 0;
        
        public static bool isFlying = false;
        
        public void Use()
        {
            CanFly = true;
        }
    }
}
