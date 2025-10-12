using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class SuperScrapJackpot : GalEffect
    {
        public string Name => "Super Scrap jackpot";
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Spawning A ton of scrap!";

        public void Use()
        {
            Networker.Instance.JackpotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(8, 12));
        }

        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
    }
}
