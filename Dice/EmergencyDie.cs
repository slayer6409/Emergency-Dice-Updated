﻿using MysteryDice.Effects;
using System.Collections;

namespace MysteryDice.Dice
{
    public class EmergencyDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start();
            DiceModel.AddComponent<Blinking>();
            this.itemProperties.verticalOffset = 0;
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

        public override IEnumerator UseTimer(ulong userID, int time)
        {
            DiceModel.GetComponent<Blinking>().BlinkingTime = 0.1f;
            return base.UseTimer(userID, time);
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

            MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
            randomEffect.Use();
            var who = !wasEnemy ? PlayerUser.playerUsername : "An Enemy";
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            
            ShowDefaultTooltip(randomEffect, diceRoll);
        }
    }
}
