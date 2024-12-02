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
    public class SurfacedDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start(); 
            DiceModel = gameObject.transform.Find("Model").gameObject;
            DiceModel.GetComponent<Spinner>().SurfacedDie = true;

        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful, EffectType.Bad});
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad, EffectType.Awful });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed, EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good, EffectType.Mixed });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good, EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great, EffectType.Good });
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public override void Roll()
        {
            var isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            var diceRoll = UnityEngine.Random.Range(1, 7);

            if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

            var randomEffect = GetRandomEffect(diceRoll, MysteryDice.SurfacedPresent? SurfacedEffects : Effects);

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
