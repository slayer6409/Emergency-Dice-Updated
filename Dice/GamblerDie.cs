using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Dice
{
    public class GamblerDie : DieBehaviour
    {
        public static bool useOutside = false;
        public override void Start()
        {
            base.Start();
            DiceModel.AddComponent<CycleSigns>();
        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Mixed, EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override IEnumerator UseTimer(ulong userID, int time)
        {
            DiceModel.GetComponent<CycleSigns>().CycleTime = 0.1f;
            return base.UseTimer(userID, time);
        }
        public override void DestroyObject()
        {
            base.DiceModel.GetComponent<CycleSigns>().HideSigns();
            base.DestroyObject();
        }

        public override void Roll()
        {
            bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            int diceRoll = UnityEngine.Random.Range(1, 7);

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

            if (randomEffect.ShowDefaultTooltip)
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            else
                Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
    }
}
