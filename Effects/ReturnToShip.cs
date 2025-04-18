using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ReturnToShip : IEffect
    {
        public string Name => "Return to ship";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Returning to ship with items!";
        public void Use()
        {
            Networker.Instance.TeleportToShipServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }

        public static void TeleportPlayerToShip(int clientID)
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Teleporting Player To Ship");
            PlayerControllerB player = null;

            player = StartOfRound.Instance.allPlayerScripts[clientID];
            if (player == null) return;
            
            
            try
            {
                if ((bool)GameObject.FindObjectOfType<AudioReverbPresets>())
                {
                    GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(player);
                }
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogDebug("Probably not an error, but: "+e.Message+"\n"+e.StackTrace);
            }
            
            player.isInElevator = true;
            player.isInHangarShipRoom = true;
            player.isInsideFactory = false;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(StartOfRound.Instance.middleOfShipNode.position, withRotation: true, 160f);
            player.beamOutParticle.Play();
           
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }
    }
}
