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
            Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(1, 3), "Jetpack");
            Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(3, 5), "Spray paint"); 
        }
    }
}
