using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class NeckBreak : IEffect
    {
        public string Name => "Neck Break";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Your neck is broken!";
        
        public static int IsNeckBroken = 0;

        public static float breakTime = 30f;
        public static bool isTimerRunning = false;
        
        public void Use()
        {
            Networker.Instance.NeckBreakRandomPlayerServerRpc(GameNetworkManager.Instance.localPlayerController.actualClientId);
        }

        public static void BreakNeck()
        {
            breakTime = UnityEngine.Random.Range(MysteryDice.minNeckBreakTimer.Value, MysteryDice.maxNeckBreakTimer.Value);
            IsNeckBroken += 1;
        }
        public static void FixNeck()
        {
            IsNeckBroken = 0;
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player == null) return;
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            if(cam != null) cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
        }
        public static IEnumerator WaitTime()
        {
            isTimerRunning = true;
            MysteryDice.CustomLogger.LogInfo($"Breaking neck for {NeckBreak.breakTime} seconds");
            yield return new WaitForSeconds(NeckBreak.breakTime);
            NeckBreak.FixNeck();
            isTimerRunning = false;
        }

    }
}
