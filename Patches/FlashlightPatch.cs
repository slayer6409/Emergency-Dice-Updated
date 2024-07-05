using HarmonyLib;
using MysteryDice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    internal class FlashlightItemPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void BrightFlashlightUpdate(FlashlightItem __instance)
        {
            if(BrightFlashlight.IsEnabled)
            {
                __instance.flashlightBulb.intensity *= 1.1f;
                __instance.flashlightBulb.spotAngle = 140f;
            } 
        }
    }
}
