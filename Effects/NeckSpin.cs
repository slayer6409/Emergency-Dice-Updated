using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class NeckSpin : IEffect
    {
        public string Name => "Neck Spin";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "ROTATION!";
        
        public static int IsNeckSpinning = 0;

        public static float minSpin = 0.01f;
        public static float maxSpin = 0.3f;
        public static float neckChoiceSpeed = 0;

        public void Use()
        {
            Networker.Instance.NeckSpinRandomPlayerServerRpc(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void SpinNeck()
        {
            neckChoiceSpeed = Random.Range(minSpin, maxSpin);
            IsNeckSpinning = 1;
        }
        public static void FixNeck()
        {
            IsNeckSpinning = 0;
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
        }
    }
}
