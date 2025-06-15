using System;
using MysteryDice.Effects;
using System.Collections;

namespace MysteryDice.Dice
{
    public class EmergencyDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override IEnumerator UseTimer(int userID, int time)
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
            
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Roll Emergency");
            int diceRoll = UnityEngine.Random.Range(1, 7);
            try
            {

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

            
                if (diceRoll > 2) randomEffect = new ReturnToShip();
                if (diceRoll == 6) randomEffect = new ReturnToShipTogether();
                
                PlaySoundBasedOnEffect(randomEffect.Outcome);

                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            
                ShowDefaultTooltip(randomEffect, diceRoll);
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
           
        }
    }
}
