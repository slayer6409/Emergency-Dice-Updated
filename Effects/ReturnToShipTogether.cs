using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ReturnToShipTogether : IEffect
    {
        public string Name => "Return to ship together";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Returning to ship with your nearest crewmates!";

        public static float DistanceToCaller = 8f;
        public void Use()
        {
            TeleportToShipTogether(StartOfRound.Instance.localPlayerController.playerClientId);
            //Networker.Instance.TeleportToShipTogetherServerRPC(StartOfRound.Instance.localPlayerController.playerClientId);
        }

        public static void TeleportToShipTogether(ulong callerID)
        {
            PlayerControllerB caller = null;
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!Misc.IsPlayerAliveAndControlled(player)) continue;

                if (callerID == player.playerClientId)
                {
                    caller = player;
                    break;
                }
            }

            if (caller == null) return;

            List<ulong> playersToTeleport = new List<ulong>();

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!Misc.IsPlayerAliveAndControlled(player)) continue;

                if (Vector3.Distance(player.transform.position, caller.transform.position) < DistanceToCaller)
                {
                    playersToTeleport.Add(player.playerClientId);
                }
            }
            foreach(var ply in playersToTeleport)
            {
                Networker.Instance.ReturnPlayerToShipServerRPC(ply);
            }
        }

    }
}
