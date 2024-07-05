using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class RebeliousCoilHeads : IEffect
    {
        public string Name => "Rebelious Coilheads";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Coilheads randomly ignore if a player is looking at them.";

        public static bool IsEnabled = false;
        public void Use()
        {
           Networker.Instance.EnableRebelServerRPC();
        }

    }
}
