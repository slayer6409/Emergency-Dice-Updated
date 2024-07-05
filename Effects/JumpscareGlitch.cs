using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class JumpscareGlitch : IEffect
    {
        public string Name => "Jumpscare";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Prepare for a jumpscare";


        public static bool PussyMode = true;
        public void Use()
        {
            Networker.Instance.StartCoroutine(Networker.Instance.DelayJumpscare());
        }
    }
}
