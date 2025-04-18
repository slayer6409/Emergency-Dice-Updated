using System;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class NeckSpin : IEffect
    {
        public string Name => "Neck Spin";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "ROTATION!";
        
        public static int IsNeckSpinning = 0;

        public static float neckChoiceSpeed = 0;

        public static int rotationNumber = 0;
        public static bool wasClimbing = false;
        public static float savedValue = 0f;
        public static int counter = 0;

        public void Use()
        {
            Networker.Instance.NeckSpinRandomPlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public static void SpinNeck()
        {
            rotationNumber = 0;
            counter = 0;
            var rotSpeed = MysteryDice.rotationSpeedModifier.Value;
            if (MysteryDice.neckRotations.Value == -1) rotSpeed = 0;

            var min = MysteryDice.minNeckSpin.Value;
            var max = MysteryDice.maxNeckSpin.Value;
            if (max < min) max = min;
            neckChoiceSpeed = Random.Range(min + rotSpeed, max + rotSpeed);
            IsNeckSpinning = 1;
        }
        public static void FixNeck()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            if(player == null) return;
            IsNeckSpinning = 0;
            Transform cam = player.gameplayCamera.transform;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
        }
    }
}
