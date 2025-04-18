using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BatteryDrain : IEffect
    {
        public string Name => "Battery Drain";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Halves the battery percentage of all items in your inventory";

        public void Use()
        {
            Networker.Instance.BatteryDrainServerRPC();
        }

        public static void removeCharge(int userID)
        {
            PlayerControllerB player = null;

            player = Misc.GetPlayerByUserID(userID);
            
            if (player == null) return;
            foreach (var item in player.ItemSlots)
            {
                if (item == null) continue;
                if (item.insertedBattery == null) continue;
                if (item.insertedBattery.empty) continue;
                item.insertedBattery.charge = item.insertedBattery.charge*.5f;
            }
            
        }
    }
}
