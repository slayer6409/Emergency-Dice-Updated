using KaimiraGames;
using MysteryDice.Effects;
using MysteryDice.Visual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Dice
{
    public class SaintDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start();
            DiceModel.transform.Find("halo").gameObject.AddComponent<HaloSpin>();
        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { });
            RollToEffect.Add(2, new EffectType[] { EffectType.Good });
            RollToEffect.Add(3, new EffectType[] { EffectType.Good });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1,7);

            if (diceRoll == 1)
            {
                Misc.SafeTipMessage($"Rolled 1", "Nothing happened");
                return;
            }

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            PlaySoundBasedOnEffect(randomEffect.Outcome);

            if(diceRoll == 6)
            {
                MysteryDice.CustomLogger.LogDebug("Rolling Effect: Saint 6");
                if(MysteryDice.NewDebugMenu.Value) DebugMenuStuff.ShowSelectEffectMenu();
                else SelectEffect.ShowSelectMenu(false,false,fromSaint:true);
                Misc.SafeTipMessage($"Rolled 6", "Choose an effect");
                var who2 = !wasEnemy ? PlayerUser.playerUsername : "An Enemy";
                Networker.Instance.LogEffectsToOwnerServerRPC(who2, randomEffect.Name, diceRoll);
                return;
            }

            MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
            randomEffect.Use();
            
            var who = !wasEnemy ? PlayerUser.playerUsername : "An Enemy";
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
    }
}
