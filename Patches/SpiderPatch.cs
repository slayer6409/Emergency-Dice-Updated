using HarmonyLib;
using MysteryDice.Effects;
using Unity.Netcode;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SpiderPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void IncreaseSpeed(SandSpiderAI __instance)
        {
            if (Arachnophobia.IsEnabled)
            {
                __instance.spiderSpeed = 12f;
                __instance.agent.speed = 12f;
                __instance.creatureAnimator.speed = 7f;
                __instance.MoveLegsProcedurally();
            }
        }
    }
}
