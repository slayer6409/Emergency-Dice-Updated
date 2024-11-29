using KaimiraGames;
using MysteryDice.Effects;


namespace MysteryDice.Dice
{
    public class ChronosDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start();
            DiceModel.AddComponent<ColorGradient>();
        }
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
            randomEffect.Use();

            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            if (isOutside && !MysteryDice.useDiceOutside.Value)
            {
                Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                return;
            }
            ShowDefaultTooltip(randomEffect, diceRoll);
        }
    }
}
