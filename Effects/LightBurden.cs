using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class LightBurden : IEffect
    {
        public string Name => "Light Burden";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Makes the weight of all items in your inventory a bit lighter";

        public void Use()
        {
            Networker.Instance.LightBurdenServerRPC();
        }


        public static void lessenWeight(int userID)
        {
            Misc.AdjustWeight(userID, .5f);
        }
    }
}
