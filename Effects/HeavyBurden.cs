using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class HeavyBurden : IEffect
    {
        public string Name => "Heavy Burden";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Makes everything in your inventory a bit heavier";

        public void Use()
        {
            Networker.Instance.HeavyBurdenServerRPC();
        }
        public static void increaseWeight(int userID)
        {
            Misc.AdjustWeight(userID, 2f);
        }
    }
}
