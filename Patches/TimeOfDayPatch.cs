using HarmonyLib;
using LethalLib;
using LethalLib.Modules;
using MysteryDice.Effects;
using MysteryDice.Visual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        public static float AdditionalBuyingRate = 0f;
        [HarmonyPostfix]
        [HarmonyPatch("SetBuyingRateForDay")]
        public static void BuyingRateMod(TimeOfDay __instance)
        {
            StartOfRound.Instance.companyBuyingRate += AdditionalBuyingRate;

            if (Networker.Instance == null) return;

            if(Networker.Instance.IsServer)
                Networker.Instance.SyncRateClientRPC(StartOfRound.Instance.companyBuyingRate);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetNewProfitQuota")]
        public static void OnNewProfitQuota(TimeOfDay __instance)
        {
            AdditionalBuyingRate = 0f;       
        }

    }
}
