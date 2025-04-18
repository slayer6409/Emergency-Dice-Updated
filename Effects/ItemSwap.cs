﻿using GameNetcodeStuff;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ItemSwap : IEffect
    {
        public string Name => "Item Swap";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Swaps the inventories of 2 players";

        public void Use()
        {
            Networker.Instance.ItemSwapServerRPC();
        }
        private static void SwapSlots(PlayerControllerB player, PlayerControllerB player2, int slot)
        {
            var p1Item = player.ItemSlots[slot];
            var p2Item = player2.ItemSlots[slot];

            // Swap the items in the slots
            player.ItemSlots[slot] = p2Item;
            player2.ItemSlots[slot] = p1Item;

            // Update HUD icons if the players are owners
            if (player.IsOwner)
            {
                HUDManager.Instance.itemSlotIcons[slot].sprite = p2Item.itemProperties.itemIcon;
                HUDManager.Instance.itemSlotIcons[slot].enabled = p2Item != null;
            }
            if (player2.IsOwner)
            {
                HUDManager.Instance.itemSlotIcons[slot].sprite = p1Item.itemProperties.itemIcon;
                HUDManager.Instance.itemSlotIcons[slot].enabled = p1Item != null;
            }

        }

        public static void itemSwap(int p1, int p2)
        {
            PlayerControllerB player1 = null;
            PlayerControllerB player2 = null;

            player1 = StartOfRound.Instance.allPlayerScripts[p1];
            player2 = StartOfRound.Instance.allPlayerScripts[p2];

            if (player1 == null || player2 == null)
            {
                Debug.LogError("One or both players not found.");
                return;
            }

            for (int i = 0; i < player1.ItemSlots.Length; i++)
            {
                SwapSlots(player1, player2, i);
            }
        }
    }
}
