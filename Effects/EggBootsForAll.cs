using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EggBootsForAll : IEffect
    {
        public string Name => "Egg Boots ALL";
        public EffectType Outcome => EffectType.GalBad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Rocket Jump!";
        public void Use()
        {
            Networker.Instance.EggBootsAllServerRpc();
        }
        
    }
}
