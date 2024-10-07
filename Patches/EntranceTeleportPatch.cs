using HarmonyLib;
using MysteryDice.Dice;
using MysteryDice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch
    {
        //[HarmonyPatch("TeleportPlayer")]
        //[HarmonyPostfix]
        //public static void FixRenderer()
        //{

        //try
        //{
        //    var player = GameNetworkManager.Instance.localPlayerController;
        //    if (player == null) return;
        //    if (player.ItemSlots.Length != 0)
        //        foreach (var item in player.ItemSlots)
        //        {
        //            if (item == null) continue;
        //            var renderer = item.GetComponent<Renderer>();
        //            if (renderer != null)
        //            {
        //                renderer.enabled = true;
        //            }
        //        }
        //}
        //catch (Exception e) 
        //{
        //    MysteryDice.CustomLogger.LogWarning(e);
        //}

        //}
    }
}
