using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AlarmCurse : IEffect
    {
        public string Name => "Alarm";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "You emit annoying sounds";

        public static bool IsCursed = false;
        public static float CursedTimer = 0f;
        public void Use()
        {
            IsCursed = true;
            CursedTimer = 3f;
        }

        public static void TimerUpdate()
        {
            if (!IsCursed) return;
            CursedTimer -= Time.deltaTime;
            if (CursedTimer < 0f)
            {
                CursedTimer = UnityEngine.Random.Range(1.5f, 20f);
                Networker.Instance.AlarmCurseServerRPC(GameNetworkManager.Instance.localPlayerController.transform.position);
            }
        }

        public static void AlarmAudio(Vector3 position) 
        {
            AudioSource.PlayClipAtPoint(MysteryDice.AlarmSFX, position, 10f);
            RoundManager.Instance.PlayAudibleNoise(position, 30f, 1f, 3, false, 0);
        }
        
    }
}
