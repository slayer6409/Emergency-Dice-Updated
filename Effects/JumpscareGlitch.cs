using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using MysteryDice.Visual;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class JumpscareGlitch : IEffect
    {
        public string Name => "Jumpscare";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Prepare for a jumpscare";

        //public static bool PussyMode = true;

        public void Use()
        {
            if (MysteryDice.JumpscareScript != null)
            {
                int index = MysteryDice.JumpscareScript.getIntNonScary();
                if (!MysteryDice.pussyMode.Value) index = MysteryDice.JumpscareScript.getIntScary();
                Networker.Instance.StartCoroutine(Networker.Instance.DelayJumpscare(index));
            }
            else
            {
                MysteryDice.CustomLogger.LogError("JumpscareScript is null when trying to use a scare index!");
            }
        }
    }
}
