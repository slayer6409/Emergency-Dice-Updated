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
            Networker.Instance.DrunkServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId,true);
        }

        public static void FixedUpdate()
        {
            if (DrunkTimer <= 0f) return;
            if (DrunkTimer > 0f)
            {
                DrunkTimer -= Time.fixedDeltaTime;
                BecomeDrunk(GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
        }
        public static void startDrinking(ulong userID)
        {
            DrunkTimer = UnityEngine.Random.Range(10,30);
        }
        public static void BecomeDrunk(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            player.drunkness = DrunkTimer;
            player.drunknessInertia = DrunkTimer;
            player.drunknessSpeed = DrunkTimer;
        }
    }
}
