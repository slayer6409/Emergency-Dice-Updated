using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Healers : IEffect
    {
        public string Name => "Maybe Healers";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "They can heal...";

        public void Use()
        {
            Networker.Instance.SpawnSurroundedServerRPC("Nancy",4,2);
            StartOfRound.Instance.StartCoroutine(setHealth());
        }

        public IEnumerator setHealth()
        {
            yield return new WaitForSeconds(1f);
            if(StartOfRound.Instance.localPlayerController.isPlayerDead) yield break;
            else StartOfRound.Instance.localPlayerController.health = 20;
        }
        
    }
}
