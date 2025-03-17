using HarmonyLib;
using LCTarrotCard.Util;
using LethalLib;
using LethalLib.Modules;
using MysteryDice.Dice;
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
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("EquipItem")]
        public static void EquipItem(GrabbableObject __instance)
        {
            if(!MysteryDice.DebugLogging.Value) return;
            // var player = GameNetworkManager.Instance?.localPlayerController;
            // if (player == null)
            // {
            //     MysteryDice.CustomLogger.LogError("Player controller is null.");
            //     return;
            // }
            // int currentSlot = player.currentItemSlot;
            // int validSlotIndex = currentSlot >= 1 ? currentSlot - 1 : currentSlot;
            // if (player.ItemSlots == null || validSlotIndex < 0 || validSlotIndex >= player.ItemSlots.Length)
            // {
            //     MysteryDice.CustomLogger.LogError($"ItemSlots are null or validSlotIndex {validSlotIndex} is out of bounds.");
            //     return;
            // }
            // var i = player.ItemSlots[validSlotIndex];
            
            var i = __instance;
            string InfoLogging = "Picked up Item: ";
            if (i.playerHeldBy.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId) return;
            if (i == null)
            {
                MysteryDice.CustomLogger.LogError($"Item is null.");
                return;
            }
            InfoLogging += i.name + " ";
            if (i.itemProperties.spawnPrefab != null)
            {
                InfoLogging += $"with prefab name {i.itemProperties.spawnPrefab.name} ";
            }
            if (i.itemProperties != null)
            {
                InfoLogging += $"and item properties name: {i.itemProperties.name} ";
            }
            MysteryDice.CustomLogger.LogInfo(InfoLogging);
        }

        
    }
}
