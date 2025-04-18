using LCTarrotCard.Ressource;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Eggs : IEffect
    {
        public string Name => "Eggs";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "They Explode?!?";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(3, 9), "Easter egg");
        }
        
    }
}
