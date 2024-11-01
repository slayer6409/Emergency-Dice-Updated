using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace MysteryDice.Dice
{
    public class GamblerDie : DieBehaviour
    {
        private Rigidbody dieRigidbody;
        public override void Start()
        {
            base.Start();
            //Maybe??? I am hoping it works! If so Thanks to Xuu!!!
            GetComponent<Item>().verticalOffset = 0;
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

        public override IEnumerator UseTimer(ulong userID, int Timer)
        {
            DiceModel.GetComponent<CycleSigns>().CycleTime = 0.1f;
            return base.UseTimer(userID, Timer);
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
    public class logPos : MonoBehaviour
    {
        public void FixedUpdate()
        {
            //gonna try here too lol
            GetComponent<Item>().verticalOffset = 0;
            if (MysteryDice.DicePosUpdate.Value) MysteryDice.CustomLogger.LogDebug(this.transform.position);
        }
    }
}
