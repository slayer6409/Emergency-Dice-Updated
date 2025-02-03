using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Rigo : IEffect
    {
        public string Name => "Duolingo Cousin";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Boom Language Learned";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, UnityEngine.Random.Range(1, 5), "Rodriguez");
        }
    }
}
