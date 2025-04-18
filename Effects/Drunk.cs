using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Drunk : IEffect
    {
        public string Name => "Drunk";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Makes you a bit drunk";

        public static float DrunkTimer = 0f;

        public void Use()
        {
            Networker.Instance.DrunkServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public static void FixedUpdate()
        {
            if (DrunkTimer <= 0f) return;
            if (DrunkTimer > 0f)
            {
                DrunkTimer -= Time.fixedDeltaTime;
                BecomeDrunk(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
            }
        }
        public static void startDrinking(int userID)
        {
            if (StartOfRound.Instance.localPlayerController != StartOfRound.Instance.allPlayerScripts[userID]) return;
            DrunkTimer = UnityEngine.Random.Range(10,30);
        }
        public static void BecomeDrunk(int userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player is null) return;
            player.drunkness = DrunkTimer;
            player.drunknessInertia = DrunkTimer;
            player.drunknessSpeed = DrunkTimer;
        }
    }
}
