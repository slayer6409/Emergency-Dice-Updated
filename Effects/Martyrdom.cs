using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Martyrdom : IEffect
    {
        public string Name => "Martyrdom";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Players Drop bombs on death now";
        
        public static bool doMinesDrop = false;

        public void Use()
        {
            Networker.Instance.MartyrdomServerRPC();
        }
    }
}
