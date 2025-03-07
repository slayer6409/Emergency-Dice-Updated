using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class HappyDay : IEffect
    {
        public string Name => "Stuffing For Days";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Lizzie's Paradise";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, UnityEngine.Random.Range(6, 12), "plushies");
        }
    }
}
