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
        public static void EquipItem()
        {
            if(!MysteryDice.DebugLogging.Value) return;
            var player = GameNetworkManager.Instance?.localPlayerController;
            if (player == null)
            {
                MysteryDice.CustomLogger.LogError("Player controller is null.");
                return;
            }
            int currentSlot = player.currentItemSlot;
            int validSlotIndex = currentSlot >= 1 ? currentSlot - 1 : currentSlot;
            if (player.ItemSlots == null || validSlotIndex < 0 || validSlotIndex >= player.ItemSlots.Length)
            {
                MysteryDice.CustomLogger.LogError($"ItemSlots are null or validSlotIndex {validSlotIndex} is out of bounds.");
                return;
            }
            var i = player.ItemSlots[validSlotIndex];
            if (i == null)
            {
                MysteryDice.CustomLogger.LogError($"Item at slot {validSlotIndex} is null.");
                return;
            }
            if (i.itemProperties == null)
            {
                MysteryDice.CustomLogger.LogError("Item properties are null.");
                return;
            }
            if (i.itemProperties.spawnPrefab == null)
            {
                MysteryDice.CustomLogger.LogError("Spawn prefab is null.");
                return;
            }
            MysteryDice.CustomLogger.LogInfo($"Picked up: {i.name} with prefab name {i.itemProperties.spawnPrefab.name}, and properties name {i.itemProperties.name}");
        }

        
    }
}
