using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Zortin : IEffect
    {
        public string Name => "Zortin";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Zort";
        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, 1, "Violin");
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, 1, "Accordion");
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, 1, "Guitar");
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, 1, "Recorder");
        }
    }
}
