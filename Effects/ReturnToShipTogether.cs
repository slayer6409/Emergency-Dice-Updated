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
            TeleportToShipTogether(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }

        public static void TeleportToShipTogether(int callerID)
        {
            PlayerControllerB caller = null;
            caller = StartOfRound.Instance.allPlayerScripts[callerID];

            if (caller == null) return;

            List<int> playersToTeleport = new List<int>();

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!Misc.IsPlayerAliveAndControlled(player)) continue;

                if (Vector3.Distance(player.transform.position, caller.transform.position) < DistanceToCaller)
                {
                    playersToTeleport.Add(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                }
            }
            foreach(var ply in playersToTeleport)
            {
                Networker.Instance.ReturnPlayerToShipServerRPC(ply);
            }
        }

    }
}
