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
            Networker.Instance.TeleportToShipServerRPC(StartOfRound.Instance.localPlayerController.playerClientId);
        }

        public static void TeleportPlayerToShip(ulong clientID)
        {
            PlayerControllerB player = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp.playerClientId == clientID)
                {
                    player = playerComp;
                    break;
                }
            }
            if (player == null) return;

            if ((bool)GameObject.FindObjectOfType<AudioReverbPresets>())
            {
                GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(player);
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
