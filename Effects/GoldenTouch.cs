using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class GoldenTouch : IEffect
    {
        public string Name => "Golden Touch";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Doubles the value of a random item in your inventory";

        public void Use()
        {
            Networker.Instance.GoldenTouchServerRPC();
        }


        public static void DoubleRandom(ulong userID)
        {
            PlayerControllerB player = null;

            // Find the player by userID
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp.playerClientId == userID)
                {
                    player = playerComp;
                    break;
                }
            }

            if (player == null)
            {
                Debug.LogError("Player not found.");
                return;
            }

            // Create a list of valid items with non-zero scrapValue
            List<GrabbableObject> validItems = new List<GrabbableObject>();

            foreach (var item in player.ItemSlots)
            {
                if (item != null && item.scrapValue > 0)
                {
                    validItems.Add(item);
                }
            }

            // Check if there are any valid items
            if (validItems.Count == 0)
            {
                Debug.LogError("No valid items with non-zero scrapValue found.");
                return;
            }

            // Select a random item from the valid items list
            int randomIndex = UnityEngine.Random.Range(0, validItems.Count);
            GrabbableObject randomItem = validItems[randomIndex];

            // Double the scrapValue of the selected item
            randomItem.scrapValue *= 2;

            Debug.Log($"Doubled the scrapValue of item {randomItem.name} to {randomItem.scrapValue}");
        }
    }
}
