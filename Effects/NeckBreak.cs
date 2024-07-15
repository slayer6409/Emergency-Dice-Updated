using GameNetcodeStuff;
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
        public void Use()
        {
            Networker.Instance.NeckBreakRandomPlayerServerRpc(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void BreakNeck()
        {
            IsNeckBroken += 1;
        }
        public static void FixNeck()
        {
            IsNeckBroken = 0;
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f);
        }
    }
}
