using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class ImmortalSnailcat : IEffect
    {
        public string Name => "Immortal Snailcat";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Immortal Snail(Cat)";

        public void Use()
        {
           Networker.Instance.doImmortalSnailCatServerRPC(1);
        }

    }
}
