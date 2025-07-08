using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class TheDesert : IEffect
    {
        public string Name => "The Desert";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You find yourself stranded in the desert...";

        public void Use()
        {
            Networker.Instance.SpawnSurroundedTrapServerRPC(DynamicTrapEffect.getTrap("Emerging Cactus 1").name,32,6);
        }
        
    }
}
