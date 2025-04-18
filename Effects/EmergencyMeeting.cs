using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EmergencyMeeting : IEffect
    {
        public string Name => "Emergency Meeting";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "EMERGENCY MEETING";
        public void Use()
        {
            Networker.Instance.EmergencyMeetingServerRPC();
        }
        public static void allEnemiesToShip()
        {
            var enemies = RoundManager.Instance.SpawnedEnemies;
            foreach (var enemy in enemies)
            {
                enemy.isOutside = true;
                enemy.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                enemy.serverPosition = StartOfRound.Instance.middleOfShipNode.position;
                Transform ClosestNodePos = enemy.ChooseClosestNodeToPosition(enemy.serverPosition, true, 0);
                if (Vector3.Magnitude(ClosestNodePos.position - enemy.serverPosition) > 10)
                {
                    enemy.serverPosition = ClosestNodePos.position;
                }
                enemy.transform.position = enemy.serverPosition;
                enemy.agent.Warp(enemy.serverPosition);
                enemy.SyncPositionToClients(); 
                if (GameNetworkManager.Instance.localPlayerController != null)
                {
                    enemy.EnableEnemyMesh(!StartOfRound.Instance.hangarDoorsClosed || !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom, false);
                }
                MysteryDice.CustomLogger.LogInfo($"Moving {enemy.name} to {enemy.serverPosition}");
            }
        }
        public static void TeleportEveryoneToShip()
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

            foreach (var ply in playersToTeleport)
            {
                Networker.Instance.TeleportPlayerToShipServerRPC(ply);
            }
        }
        public static void TeleportPlayerToShip(int who)
        {
            PlayerControllerB player = null;

            player = StartOfRound.Instance.allPlayerScripts[who];

            if (player == null) return;

            var reverbPresets = GameObject.FindObjectOfType<AudioReverbPresets>();
            try
            {
                if ((bool)GameObject.FindObjectOfType<AudioReverbPresets>())
                {
                    GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(player);
                }
            }
            catch (Exception e)
            {
                
            }

            player.isInElevator = false;
            player.isInHangarShipRoom = true;
            player.isInsideFactory = false;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(StartOfRound.Instance.middleOfShipNode.position);
            player.beamOutParticle.Play();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }
    }
}
