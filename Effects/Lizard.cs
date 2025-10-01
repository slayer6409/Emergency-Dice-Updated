using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using MysteryDice.Gal;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Lizard : IEffect
    {
        public string Name => "Lizard";
        public EffectType Outcome => EffectType.GalMixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Lizard Lizard Lizard Lizard";

        public static bool isRunning = false;

        public static double nextTime = 0;
        public void Use()
        {
            Networker.Instance.DoLizardServerRPC();
        }

        public static void FixedUpdate()
        {
            if (!isRunning) return;
            if (nextTime <= 0)
            {
                nextTime = UnityEngine.Random.Range(10f, 60f);
                DoLizardStuff();
            }
            nextTime -= Time.fixedDeltaTime;
        }

        public static void DoLizardStuff()
        {
            if(DiceGalAI.Instances[0] == null) return;
            var gal = DiceGalAI.Instances[0] as DiceGalAI;
            if(gal == null) return;
            Networker.Instance.PlaySoundFromGalServerRPC("Lizard");
            Networker.Instance.SpawnEnemyAtPosServerRPC("Tulip Snake", gal.transform.position);
        }
    }
}
