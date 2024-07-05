using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Swap : IEffect
    {
        public string Name => "Swap";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Swapping two players!";

        public void Use()
        {
            Networker.Instance.SwapPlayersServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void SwapPlayers(ulong p1ID, ulong p2ID)
        {

            PlayerControllerB p1 = null, p2 = null;
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (player.playerClientId == p1ID)
                    p1 = player;
                if (player.playerClientId == p2ID)
                    p2 = player;
            }
            if (p1 == null || p2 == null) return;

            Vector3 pos1 = p1.transform.position;
            Vector3 pos2 = p2.transform.position;

            bool P1Elevator = p1.isInElevator;
            bool P1Hangar = p1.isInHangarShipRoom;
            bool P1Factory = p1.isInsideFactory;

            p1.isInElevator = p2.isInElevator;
            p1.isInHangarShipRoom = p2.isInHangarShipRoom;
            p1.isInsideFactory = p2.isInsideFactory;

            p2.isInElevator = P1Elevator;
            p2.isInHangarShipRoom = P1Hangar;
            p2.isInsideFactory = P1Factory;

            p1.beamOutParticle.Play();
            p2.beamOutParticle.Play();
            p1.TeleportPlayer(pos2);
            p2.TeleportPlayer(pos1);
        }
    }
}
