using GameNetcodeStuff;
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
        public static void itemSwap(ulong p1, ulong p2)
        {
            PlayerControllerB player1 = null;
            PlayerControllerB player2 = null;

            // Find both players in the list
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp.playerClientId == p1)
                {
                    player1 = playerComp;
                }
                else if (playerComp.playerClientId == p2)
                {
                    player2 = playerComp;
                }

                // Exit the loop early if both players are found
                if (player1 != null && player2 != null)
                {
                    break;
                }
            }

            // Ensure both players are found
            if (player1 == null || player2 == null)
            {
                Debug.LogError("One or both players not found.");
                return;
            }

            for (int i = 0; i < player1.ItemSlots.Length; i++)
            {
                GrabbableObject temp = player1.ItemSlots[i];
                player1.ItemSlots[i] = player2.ItemSlots[i];
                player2.ItemSlots[i] = temp;
            }
        }
    }
}
