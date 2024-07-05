using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TurnOffLights : IEffect
    {
        public string Name => "Lights off";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Turns off all lights";

        public void Use()
        {
            Networker.Instance.TurnOffAllLightsServerRPC();
        }
    }
}
