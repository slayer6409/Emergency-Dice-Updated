using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AlarmCurse : IEffect
    {
        public string Name => "Alarm";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Do you hear that? ʘ‿ʘ";

        public static bool IsCursed = false;
        public static float CursedTimer = 0f;
        public static ConfigEntry<bool> fireAlarm;
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
                if(fireAlarm.Value) CursedTimer = UnityEngine.Random.Range(10.5f, 20f);
                else CursedTimer = UnityEngine.Random.Range(1.5f, 20f);
                Networker.Instance.AlarmCurseServerRPC(GameNetworkManager.Instance.localPlayerController.transform.position);
            }
        }

        public static void AlarmAudio(Vector3 position)
        {
            AudioClip clip = MysteryDice.AlarmSFX;
            if(fireAlarm.Value) clip = MysteryDice.FireAlarmSFX;
            AudioSource.PlayClipAtPoint(clip, position, 10f);
            RoundManager.Instance.PlayAudibleNoise(position, 30f, 1f, 3, false, 0);
        }

        public static void Config()
        {
            fireAlarm = MysteryDice.BepInExConfig.Bind<bool>(
                "Alarm",
                "Use Alt Sound",
                true,
                "Use an Alternate Sound");
        }
        
    }
}
