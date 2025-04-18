﻿using LCTarrotCard.Ressource;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TakeySmol : IEffect
    {
        public string Name => "Takey Plushies";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "What do they do?";

        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(3, 7), "takey");
        }
    }
}
