using System;
using MysteryDice.Effects;
using System.Collections;

namespace MysteryDice.Dice
{
    public class GamblerDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad, EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Mixed, EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override IEnumerator UseTimer(int userID, int Timer)
        {
            DiceModel.GetComponent<CycleSigns>().CycleTime = 0.1f;
            return base.UseTimer(userID, Timer);
        }
        public override void DestroyObject()
        {
            base.DiceModel.GetComponent<CycleSigns>().HideSigns();
            base.DestroyObject();
        }

        public override void Roll()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Roll Gambler");
            try
            {
                bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                int diceRoll = UnityEngine.Random.Range(1, 7);

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
            
                var who = wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
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