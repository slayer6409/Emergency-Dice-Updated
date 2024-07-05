using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ModifyPitch : IEffect
    {
        public string Name => "Speech jammer";
        const float PitchSwitchTime = 0.05f;
        private static float PitchSwitchTimer = 0f;
        public static bool FluctuatePitch = false;
        private static float CumulativeRandomFreq = 0f;
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Good luck communicating";
        public void Use()
        {
            Networker.Instance.ModifyPitchNotifyServerRPC();
        }

        public static void ResetPitch()
        {
            FluctuatePitch = false;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                SoundManager.Instance.SetPlayerPitch(1f, i);
        }

        public static void PitchFluctuate()
        {
            if (!FluctuatePitch) return;

            CumulativeRandomFreq += UnityEngine.Random.Range(-0.5f, 0.5f) * Time.deltaTime;
            CumulativeRandomFreq = Mathf.Clamp(CumulativeRandomFreq, -1f, 1f);
            PitchSwitchTimer -= Time.deltaTime;
            if (PitchSwitchTimer <= 0f)
            {
                try
                {
                    PitchSwitchTimer = PitchSwitchTime;
                    for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                        SoundManager.Instance.SetPlayerPitch(
                            Misc.Map(Mathf.Sin((2f + CumulativeRandomFreq) * Mathf.PI * Time.time)
                            , -1f, 1f, 0.6f, 1.4f), i);
                }
                catch(Exception ex)
                {
                    MysteryDice.CustomLogger.LogWarning($"There's a problem with the pitch fluctuate effect: {ex.Message}");
                }
                
            }
        }
    }
}
