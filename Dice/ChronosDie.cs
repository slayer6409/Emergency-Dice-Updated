using KaimiraGames;
using MysteryDice.Effects;


namespace MysteryDice.Dice
{
    public class ChronosDie : DieBehaviour
    {
        public static bool useOutside = false;
        public override void Start()
        {
            base.Start();
            DiceModel.AddComponent<ColorGradient>();
        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
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
            weightedRolls.Add(1, 1 + (int)(offset * 10f));
            weightedRolls.Add(2, 1 + (int)(offset * 8f));
            weightedRolls.Add(3, 1 + (int)(offset * 6f));
            weightedRolls.Add(4, 1 + (int)(offset * 3f));
            weightedRolls.Add(5, 1 + (int)(offset * 1f));
            weightedRolls.Add(6, 1);

            bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            int diceRoll = weightedRolls.Next();

            if (isOutside && !useOutside) diceRoll = 1;

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();

            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            if (isOutside && !useOutside)
            {
                Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                return;
            }
<<<<<<< Updated upstream

            if (randomEffect.ShowDefaultTooltip)
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            else
                Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
=======
                ShowDefaultTooltip(randomEffect, diceRoll);
            }
        
>>>>>>> Stashed changes
    }
}
