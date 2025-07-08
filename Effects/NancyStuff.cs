using LethalLib.Modules;
using MysteryDice.Patches;
using System;
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
            StartOfRound.Instance.localPlayerController.health = 20;
        }
        
    }
}
