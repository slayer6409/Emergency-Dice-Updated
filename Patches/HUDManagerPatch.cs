using System.Collections.Generic;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace MysteryDice.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal class HudManagerPatch
{
    [HarmonyPatch("displayAd")]
    [HarmonyPrefix]
    public static void DeadAd(HUDManager __instance)
    {
        if(!MysteryDice.deadAds.Value) return;
        if(StartOfRound.Instance.localPlayerController.isPlayerDead) __instance.advertAnimator.SetTrigger("PopUpAd");
    }
}
