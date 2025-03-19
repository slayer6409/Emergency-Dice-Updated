using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
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


        public static int GetRandomItem(ulong userID)
        {
            PlayerControllerB player = null;

            // Find the player by userID
            player = Misc.GetRandomAlivePlayer();

            if (player == null)
            {
                MysteryDice.CustomLogger.LogError("Player not found.");
                return -1;
            }

            Dictionary<GrabbableObject, int> validItems = new Dictionary<GrabbableObject, int>();

            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                var item = player.ItemSlots[i];
                if (item != null && item.scrapValue > 0)
                {
                    validItems.Add(item, i);
                }
            }

            // Check if there are any valid items
            if (validItems.Count == 0)
            {
                Debug.LogError("No valid items with non-zero scrapValue found.");
                return -1;
            }

            int randomIndex = UnityEngine.Random.Range(0, validItems.Count);
            var randomItem = validItems.ElementAt(randomIndex);

            return randomItem.Value;
        }
    }
}
