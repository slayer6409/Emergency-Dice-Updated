using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class HyperShake : IEffect
    {
        public string Name => "Hyper Shake";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Makes you shake a lot!";

        public class ShakeData
        {
            public PlayerControllerB Player;
            public float NextShakeTimer;
            public float ShakingTimer;
            public float Force = 0f;
        }

        public static ShakeData ShakingData = null;

        public void Use()
        {
            Networker.Instance.HyperShakeServerRPC();
        }

        public static void FixedUpdate()
        {
            if (ShakingData == null) return;

            ShakingData.NextShakeTimer -= Time.fixedDeltaTime;

            if(ShakingData.NextShakeTimer < 0f)
            {
                ShakingData.ShakingTimer = Random.Range(2f, 4f);
                ShakingData.NextShakeTimer = Random.Range(5f, 40f);
                var min = MysteryDice.minHyperShake.Value;
                var max = MysteryDice.maxHyperShake.Value;
                if(max<min)max = min;
                ShakingData.Force = Random.Range(min, max);
            }
            if(ShakingData.ShakingTimer > 0f)
            {
                ShakingData.ShakingTimer -= Time.fixedDeltaTime;
                Shake(ShakingData);
            }
        }

        public static void Shake(ShakeData data)
        {
            Vector3 randDir = new Vector3(Random.Range(-5f, 5f)*Mathf.Sin(Time.time), Random.Range(-0.01f,1f), Random.Range(-5f, 5f) * Mathf.Sin(Time.time));
            data.Player.externalForces += randDir*data.Force;
        }
    }
}
