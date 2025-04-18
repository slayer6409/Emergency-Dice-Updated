using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

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
        public static ConfigEntry<bool> HorribleVersion;
        public static List<AudioClip> alarmSounds = new List<AudioClip>();

        public void Use()
        {
            var glitch = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(x => x.playerSteamId == 76561198984467725);
            if ( glitch != null)
            {
                if (!glitch.isPlayerDead) Networker.Instance.AlarmServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, glitch));
                else Networker.Instance.AlarmServerRPC(Misc.GetRandomPlayerID());
            }
            else
            {
                Networker.Instance.AlarmServerRPC(Misc.GetRandomPlayerID());
            }
        }

        public static void setAlarm()
        {
            IsCursed = true;
            CursedTimer = 3f;
        }

    public static void TimerUpdate()
        {
            if (alarmSounds.Count == 0)
            {
                AudioClip tempClip;
                MysteryDice.sounds.TryGetValue("FireAlarm", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("FireAlarm Null");
                MysteryDice.sounds.TryGetValue("WindowsError", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("WindowsError Null");
                MysteryDice.sounds.TryGetValue("disconnect", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("disconnect Null");
                MysteryDice.sounds.TryGetValue("DoorLeft", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("DoorLeft Null");
                MysteryDice.sounds.TryGetValue("DoorRight", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("DoorRight Null");
                MysteryDice.sounds.TryGetValue("AudioTest", out tempClip);
                if (tempClip != null)alarmSounds.Add(tempClip); else Debug.LogWarning("AudioTest Null");
            }
            if (!IsCursed) return;
            CursedTimer -= Time.deltaTime;
            if (CursedTimer < 0f)
            {
                if(fireAlarm.Value) CursedTimer = UnityEngine.Random.Range(10.5f, 20f);
                else CursedTimer = UnityEngine.Random.Range(1.5f, 20f);
                bool isGlitch = GameNetworkManager.Instance.localPlayerController.playerSteamId == 76561198984467725;
                var audioInt = new Random().Next(0, alarmSounds.Count);
                Networker.Instance.AlarmCurseServerRPC(GameNetworkManager.Instance.localPlayerController.transform.position,audioInt, isGlitch);
            }
        }

        public static void AlarmAudio(Vector3 position, int audioNum = 0, bool isGlitch = false)
        {
            MysteryDice.sounds.TryGetValue("alarmcurse", out AudioClip clip);
            if (fireAlarm.Value) MysteryDice.sounds.TryGetValue("FireAlarm", out clip);

            if (HorribleVersion.Value || isGlitch)
            {
                clip = alarmSounds[audioNum];
            }
            //if(clip != null) Debug.Log($"Playing alarm sound{clip.name}");
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
            HorribleVersion = MysteryDice.BepInExConfig.Bind<bool>(
                "Alarm",
                "Use Horrible Version",
                false,
                "Makes it worse");
        }
    }
}
