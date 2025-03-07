using GameNetcodeStuff;
using MysteryDice.Dice;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Delay : IEffect
    {
        public string Name => "Delayed Reaction";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Wait for it..."; 
        public void Use()
        {
            Networker.Instance.DelayedReactionServerRPC(Misc.GetRandomPlayerID());
        }


        public static void DelayedReaction(ulong userID)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player.actualClientId != userID) return;
            int e = UnityEngine.Random.Range(0, 7);
            IEffect randomEffect = GetRandomEffect(e); 
            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();
            var who = player != null ? player.playerUsername : "An Enemy";
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, e);
            Misc.SafeTipMessage($"NOW", randomEffect.Tooltip);
        }
        public static void PlaySoundBasedOnEffect(EffectType effectType)
        {
            AudioClip clip;
            switch (effectType)
            {
                case EffectType.Awful:
                    MysteryDice.sounds.TryGetValue("Bell2", out clip);
                    break;
                case EffectType.Bad:
                    MysteryDice.sounds.TryGetValue("Bad1", out clip);
                    break;
                default:
                    MysteryDice.sounds.TryGetValue("Good2", out clip);
                    break;
            }

            AudioSource.PlayClipAtPoint(clip, GameNetworkManager.Instance.localPlayerController.transform.position);
        }
        public static IEffect GetRandomEffect(int diceRoll)
        {
            List<IEffect> rolledEffects = new List<IEffect>();

            switch (diceRoll)
            {
                case 0:
                    rolledEffects.AddRange(DieBehaviour.AwfulEffects);
                    break;
                case 1:
                    rolledEffects.AddRange(DieBehaviour.AwfulEffects);
                    rolledEffects.AddRange(DieBehaviour.BadEffects);
                    break;
                case 2:
                    rolledEffects.AddRange(DieBehaviour.BadEffects);
                    break;
                case 3:
                    rolledEffects.AddRange(DieBehaviour.MixedEffects);
                    break;
                case 4:
                    rolledEffects.AddRange(DieBehaviour.GoodEffects);
                    rolledEffects.AddRange(DieBehaviour.MixedEffects);
                    break;
                case 5:
                    rolledEffects.AddRange(DieBehaviour.GoodEffects);
                    break;
                case 6:
                    rolledEffects.AddRange(DieBehaviour.GoodEffects);
                    rolledEffects.AddRange(DieBehaviour.GreatEffects);
                    break;
            }

            if (rolledEffects.Count == 0) return null;
            int randomEffectID = UnityEngine.Random.Range(0, rolledEffects.Count);
            return rolledEffects[randomEffectID];
        }
    }
}
