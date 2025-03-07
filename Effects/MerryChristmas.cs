using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class MerryChristmas : IEffect
    {
        public string Name => "Christmas";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Merry Christmas!";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, UnityEngine.Random.Range(3, 9), "Gift");
        }
    }
}
