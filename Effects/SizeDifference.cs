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
    internal class SizeDifference : IEffect
    {
        public string Name => "Become Small";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You are small, do /visor to toggle visor";

        public static ConfigEntry<sizeRevert> sizeOption;

        public static bool hudGoneBefore = false;

        public enum sizeRevert
        {
            after,
            close,
            again,
            bothAgainAfter
        }

        public void Use()
        {
            Networker.Instance.BecomeSmallServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void fixSize(ulong userID) 
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            player.transform.localScale = Vector3.one; 
        }
        public static void BecomeSmall(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            if(player.transform.localScale != Vector3.one)
            {
                if(sizeOption.Value == sizeRevert.again|| sizeOption.Value == sizeRevert.bothAgainAfter) Networker.Instance.fixSizeServerRPC(userID);
                return;
            }
            player.transform.localScale = new Vector3(0.3333f, 0.33333f, 0.333333f);
        }
        public static void ToggleVisor()
        {
            GameObject visor = GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/ScavengerHelmet");
            MeshRenderer component = visor.GetComponent<MeshRenderer>();
            component.enabled = !component.enabled;
        }
        public static void Config()
        {
            sizeOption = MysteryDice.BepInExConfig.Bind<sizeRevert>(
               "SizeDifference",
               "Size Change Option",
               sizeRevert.close,
               "Determines when the size difference reverts back to normal. \nAfter - Sets it to after you take off.\nClose - Sets it to after the game/lobby closes \nAgain - Makes it to where you have to get the effect again \nBothAgainAfter - Is both Again and After, so you will revert after or if you get the effect again (default) ");
           
        }
    }
}
