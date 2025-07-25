using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class MadScience : IEffect
    {
        public string Name => "Mad Science";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "IT'S ALIVE! ALIVE!!!!!!!!!!!!!!";

        public void Use()
        {
            Networker.Instance.doMadScienceServerRPC(1);
        }
    }
    internal class MadderScience : IEffect
    {
        public string Name => "Madder Science";
        public EffectType Outcome => EffectType.GalAwful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "MUWAHAHAHAHA";

        public void Use()
        {
            Networker.Instance.doMadScienceServerRPC(3);
        }
    }
}
