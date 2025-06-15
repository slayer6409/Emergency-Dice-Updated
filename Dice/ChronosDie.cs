using System;
using KaimiraGames;
using MysteryDice.Effects;


namespace MysteryDice.Dice
{
    public class ChronosDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed, EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Bad, EffectType.Good, EffectType.Great });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good, EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override void DestroyObject()
        {
            Destroy(DiceModel.GetComponent<ColorGradient>());
            base.DestroyObject();
        }

        public override void Roll()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Roll Chronos");
            try
            {
                float offset = TimeOfDay.Instance.normalizedTimeOfDay;
                WeightedList<int> weightedRolls = new WeightedList<int>();
                if (!MysteryDice.chronosUpdatedTimeOfDay.Value) 
                {
                    weightedRolls.Add(1, 1 + (int)(offset * 10f));
                    weightedRolls.Add(2, 1 + (int)(offset * 8f));
                    weightedRolls.Add(3, 1 + (int)(offset * 6f));
                    weightedRolls.Add(4, 1 + (int)(offset * 3f));
                    weightedRolls.Add(5, 1 + (int)(offset * 1f));
                    weightedRolls.Add(6, 1);
                }
                else if (MysteryDice.chronosUpdatedTimeOfDay.Value)
                {
                    if(offset < .5f)
                    {

                        weightedRolls.Add(1, 1 + (int)((1 - offset) * 10f));
                        weightedRolls.Add(2, 1 + (int)((1 - offset) * 8f));
                        weightedRolls.Add(3, 1 + (int)((1 - offset) * 6f));
                        weightedRolls.Add(4, 1 + (int)(offset * 4f));
                        weightedRolls.Add(5, 1 + (int)(offset * 2f));
                        weightedRolls.Add(6, 1 + (int)offset);
                    }
                    else if(offset >= .5f)
                    {
                        weightedRolls.Add(1, 1 + (int)(offset * 4f));
                        weightedRolls.Add(2, 1 + (int)(offset * 2f));
                        weightedRolls.Add(3, 1 + (int)offset);
                        weightedRolls.Add(4, 1 + (int)((1 - offset) * 6f));
                        weightedRolls.Add(5, 1 + (int)((1 - offset) * 8f));
                        weightedRolls.Add(6, 1 + (int)((1 - offset) * 10f));
                    }
                }

                bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                int diceRoll = weightedRolls.Next();

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
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
