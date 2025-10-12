using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EggBootsForAll : GalEffect
    {
        public string Name => "Egg Boots ALL";
        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Rocket Jump!";
        public void Use()
        {
            Networker.Instance.EggBootsAllServerRpc();
        }
        
    }
}
