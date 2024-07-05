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
    public class RustyDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { });
            RollToEffect.Add(4, new EffectType[] { });
            RollToEffect.Add(5, new EffectType[] { });
            RollToEffect.Add(6, new EffectType[] { });
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1,7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if(randomEffect == null)
            {
                switch (diceRoll)
                {
                    case 3:
                        Networker.Instance.JackpotServerRPC(PlayerUser.actualClientId, UnityEngine.Random.Range(1, 2));
                        Misc.SafeTipMessage($"Rolled 3", "Spawning some scrap");
                        break;
                    case 4:
                        Networker.Instance.JackpotServerRPC(PlayerUser.actualClientId, UnityEngine.Random.Range(3, 4));
                        Misc.SafeTipMessage($"Rolled 4", "Spawning scrap");
                        break;
                    case 5:
                        Networker.Instance.JackpotServerRPC(PlayerUser.actualClientId, UnityEngine.Random.Range(5, 6));
                        Misc.SafeTipMessage($"Rolled 5", "Spawning more scrap");
                        break;
                    case 6:
                        Networker.Instance.JackpotServerRPC(PlayerUser.actualClientId, UnityEngine.Random.Range(7, 8));
                        Misc.SafeTipMessage($"Rolled 6", "Spawning a lot of scrap!");
                        break;
                }
            }
            else
            {
                PlaySoundBasedOnEffect(randomEffect.Outcome);
                randomEffect.Use();
                Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            }
            
        }
    }
}
