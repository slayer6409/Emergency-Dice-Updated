using System;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace MysteryDice.Effects
{
    internal class TargetPractice : IEffect
    {
        public string Name => "Target Practice";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You are now a target!";

        public void Use()
        {
            Networker.Instance.targetPracticeServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }
    }
}
