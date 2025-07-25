using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class MovingCrane : IEffect
    {
        public string Name => "Moving Crane";
        public EffectType Outcome => EffectType.GalAwful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Dear Lord what have I done";

        public void Use()
        {
            Networker.Instance.doMovingCraneServerRPC();
        }
    }
   
}
