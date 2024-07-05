using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Armageddon : IEffect
    {
        public string Name => "Armageddon";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Armageddon outside!";

        public static bool IsEnabled = false;
        public static float TimeToBoom = 0f;
        public void Use()
        {
            Networker.Instance.SetArmageddonServerRPC(true);
        }

        public static void BoomTimer()
        {
            if (!IsEnabled) return;
            TimeToBoom -= Time.fixedDeltaTime;

            if (TimeToBoom <= 0f)
            {
                Vector3 position = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
                Networker.Instance.DetonateAtPosClientRPC(position);
                TimeToBoom = UnityEngine.Random.Range(0.3f, 2f);
            }
        }
    }
}
