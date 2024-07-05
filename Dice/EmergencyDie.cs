using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Dice
{
    public class EmergencyDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start();
            DiceModel.AddComponent<Blinking>();
        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override IEnumerator UseTimer(ulong userID)
        {
            DiceModel.GetComponent<Blinking>().BlinkingTime = 0.1f;
            return base.UseTimer(userID);
        }

        public override void DestroyObject()
        {
            DiceModel.GetComponent<Blinking>().HideSigns();
            base.DestroyObject();
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1, 7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);

            if (diceRoll > 3) randomEffect = new ReturnToShip();
            if (diceRoll == 6) randomEffect = new ReturnToShipTogether();

            randomEffect.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            if (randomEffect.ShowDefaultTooltip)
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            else
                Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
    }
}
