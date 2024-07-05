using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Detonate : IEffect
    {
        public string Name => "Detonate";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "A random player will soon explode";

        public void Use()
        {
            Networker.Instance.DetonateRandomPlayerServerRpc();
        }
    }
}
