using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using MysteryDice.Dice;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class DoubleTrouble : IEffect
    {
        public string Name => "Double Trouble";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "This could either be good or bad...";

        public void Use()
        {
            int roll = -1;
            string who = StartOfRound.Instance.localPlayerController.playerUsername;
            IEffect effect = DieBehaviour.AllowedEffects.ToList().OrderBy(_ => UnityEngine.Random.value).First();
            MysteryDice.CustomLogger.LogDebug("Rolling Effect in Double Trouble: "+ effect.Name);
            effect.Use(); 
            Networker.Instance.LogEffectsToOwnerServerRPC(who, effect.Name, roll);
            
            IEffect effect2 = DieBehaviour.AllowedEffects.ToList().OrderBy(_ => UnityEngine.Random.value).First();
            MysteryDice.CustomLogger.LogDebug("Rolling Effect in Double Trouble: "+ effect2.Name);
            effect2.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(who, effect2.Name, roll);
        }
    }
}
