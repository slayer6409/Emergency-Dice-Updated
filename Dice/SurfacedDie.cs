using System;
using MysteryDice.Effects;

namespace MysteryDice.Dice
{
    public class SurfacedDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, [EffectType.Awful, EffectType.Bad,EffectType.Mixed]);
            RollToEffect.Add(2, [EffectType.Bad, EffectType.Awful,EffectType.Mixed]);
            RollToEffect.Add(3, [EffectType.Mixed, EffectType.Bad, EffectType.Good]);
            RollToEffect.Add(4, [EffectType.Good, EffectType.Mixed, EffectType.Bad]);
            RollToEffect.Add(5, [EffectType.Good, EffectType.Great,EffectType.Mixed]);
            RollToEffect.Add(6, [EffectType.Great, EffectType.Good,EffectType.Mixed]);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public override void Roll()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Roll Surfaced");
            try
            {
                var isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                var diceRoll = UnityEngine.Random.Range(1, 7);

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                var randomEffect = GetRandomEffect(diceRoll, MysteryDice.SurfacedPresent? SurfacedEffects : Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
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
