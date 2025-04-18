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
            TeleportEveryoneToSomeone(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }

        public static void TeleportEveryoneToSomeone(int callerID)
        {
            List<int> playersToTeleport = new List<int>();
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (Misc.IsPlayerAliveAndControlled(player))
                {
                    playersToTeleport.Add(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                }
            }

            if (playersToTeleport.Count == 0) return;

            PlayerControllerB randomPlayer = Misc.GetRandomAlivePlayer();
            if (randomPlayer == null) return;

            int toTpTo = Array.IndexOf(StartOfRound.Instance.allPlayerScripts,randomPlayer);

            foreach (var ply in playersToTeleport)
            {
                if (ply != toTpTo)
                {
                    Networker.Instance.TeleportToPlayerServerRPC(ply, toTpTo);
                }
            }
        }

        public static void TeleportPlayerToPlayer(int who, int toWhom)
        {
            PlayerControllerB player = null;
            PlayerControllerB player2 = null;

            player = StartOfRound.Instance.allPlayerScripts[who];
            player2 = StartOfRound.Instance.allPlayerScripts[toWhom];

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
