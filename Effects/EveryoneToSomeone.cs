using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EveryoneToSomeone : IEffect
    {
        public string Name => "Everyone To Someone";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Sending Everyone to Someone!";
        public void Use()
        {
            TeleportEveryoneToSomeone(StartOfRound.Instance.localPlayerController.playerClientId);
        }

        public static void TeleportEveryoneToSomeone(ulong callerID)
        {
            List<ulong> playersToTeleport = new List<ulong>();
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (Misc.IsPlayerAliveAndControlled(player))
                {
                    playersToTeleport.Add(player.playerClientId);
                }
            }

            if (playersToTeleport.Count == 0) return;

            PlayerControllerB randomPlayer = Misc.GetRandomAlivePlayer();
            if (randomPlayer == null) return;

            ulong toTpTo = randomPlayer.playerClientId;

            foreach (var ply in playersToTeleport)
            {
                if (ply != toTpTo)
                {
                    Networker.Instance.TeleportToPlayerServerRPC(ply, toTpTo);
                }
            }
        }

        public static void TeleportPlayerToPlayer(ulong who, ulong toWhom)
        {
            PlayerControllerB player = null;
            PlayerControllerB player2 = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp == null) continue;

                if (playerComp.playerClientId == who)
                {
                    player = playerComp;
                }
                else if (playerComp.playerClientId == toWhom)
                {
                    player2 = playerComp;
                }

                if (player != null && player2 != null)
                {
                    break;
                }
            }

            if (player == null || player2 == null) return;

            var reverbPresets = GameObject.FindObjectOfType<AudioReverbPresets>();
            if (reverbPresets != null)
            {
                reverbPresets.audioPresets[3].ChangeAudioReverbForPlayer(player);
            }

            player.isInElevator = player2.isInElevator;
            player.isInHangarShipRoom = player2.isInHangarShipRoom;
            player.isInsideFactory = player2.isInsideFactory;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(player2.transform.position);
            player.beamOutParticle.Play();

            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }
    }
}
