using KaimiraGames;
using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Dice
{
    public class SacrificerDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(3, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(5, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(6, new EffectType[] { EffectType.Bad });
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1,7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            if (diceRoll == 1)
            {
                Misc.SafeTipMessage($"Rolled 1...", "Run");
                randomEffect = GetRandomEffect(diceRoll, Effects);
                randomEffect.Use();
                Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);
            }
            else
                Misc.SafeTipMessage($"Rolled {diceRoll}", EffectText(randomEffect.Outcome));

            new ReturnToShip().Use();
        }

        public string EffectText(EffectType effectType)
        {
            if (effectType == EffectType.Bad)
                return "This could have been worse";
            if (effectType == EffectType.Awful)
                return "Terrible";
            return "";
        }
    }
}
