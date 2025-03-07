using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class DrunkForAll : IEffect
    {
        public string Name => "$Drunk All";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Makes you a bit drunk";

        public static float DrunkTimer = 0f;

        public void Use()
        {
            Networker.Instance.DrunkServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId,true);
        }

        public static void startDrinking(ulong userID)
        {
            Drunk.DrunkTimer = UnityEngine.Random.Range(10,30);
        }
    }
}
