using BepInEx.Configuration;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class SizeDifferenceForAll : IEffect
    {
        public string Name => "$Become Small For All";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You are small, do /visor to toggle visor";


        public static bool hudGoneBefore = false;

        public void Use()
        {
            Networker.Instance.AllPlayerUseServerRPC();
        }

        public static void BecomeSmall(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            if (player.transform.localScale != Vector3.one)
            {
                Misc.SafeTipMessage("Fixed your size", ":D");
                Networker.Instance.fixSizeServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
                return;
            }
            else
            {
                Misc.SafeTipMessage("You are small", "do /visor to toggle visor");
            }
            player.transform.localScale = new Vector3(0.3333f, 0.33333f, 0.333333f);
        }
        
    }
}
