using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class DorjesDream : IEffect
    {
        public string Name => "Dorjes Dream";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Dreams of Dorje";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, UnityEngine.Random.Range(1, 3), "Jetpack");
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, UnityEngine.Random.Range(3, 5), "Spray paint"); 
        }
    }
}
