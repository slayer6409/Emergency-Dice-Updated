using System;
using System.Collections;
using MysteryDice.Effects;
using MysteryDice.Patches;
using UnityEngine;

namespace MysteryDice.Dice
{
    public class SteveLeDice : DieBehaviour
    {
        public OwnerNetworkAnimator NetworkAnimator;
        public Animator Animator;
        
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, [EffectType.Awful, EffectType.Bad]);
            RollToEffect.Add(2, [EffectType.Bad, EffectType.Awful,EffectType.Mixed]);
            RollToEffect.Add(3, [EffectType.Mixed, EffectType.Bad, EffectType.Good]);
            RollToEffect.Add(4, [EffectType.Good, EffectType.Mixed, EffectType.Bad]);
            RollToEffect.Add(5, [EffectType.Good, EffectType.Great,EffectType.Mixed]);
            RollToEffect.Add(6, [EffectType.Great, EffectType.Good]);
        }

        public override IEnumerator UseTimer(int userID, int spinTime)
        {
            if (IsOwner)
            {
                NetworkAnimator.SetTrigger("use");
                Animator.SetBool("useBool", true);
            }
            
            Networker.Instance.RequestEmotesInRadiusServerRpc(transform.position, 10);
            yield return StartCoroutine(base.UseTimer(userID, 5));
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if(!IsOwner) return;
            Animator.SetBool("isHeld", true);
        }
        
        public override void DiscardItem()
        {
            base.DiscardItem();
            if(!IsOwner) return;
            Animator.SetBool("isHeld", false);
        }
        
        public override void Roll()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Roll Steve");
            try
            {
                var isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                var diceRoll = UnityEngine.Random.Range(1, 7);

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                var randomEffect = GetRandomEffect(diceRoll, SteveEffects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
            
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                if (isOutside && !MysteryDice.useDiceOutside.Value)
                {
                    Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                    return;
                }
                ShowDefaultTooltip(randomEffect, diceRoll);
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
        }
    }
}
