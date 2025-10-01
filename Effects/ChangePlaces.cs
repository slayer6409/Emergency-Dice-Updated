using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class ChangePlaces : IEffect
    {
        public string Name => "Change Places";
        public EffectType Outcome => EffectType.GalMixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Musical Chairs gone wrong";
        public static bool doingStuff = false;
        private static float doNext = 0;
        
        public void Use()
        {
            Networker.Instance.doSwapperServerRPC();
        }

        public static void SwapPlayers()
        {
            Dictionary<PlayerControllerB, Vector3> playerList = new Dictionary<PlayerControllerB, Vector3>();
            List<int> playerIDs = new List<int>();
            List<Vector3> playerPositions = new List<Vector3>();
            List<bool> inside = new List<bool>();
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if(player.isPlayerControlled) playerList.Add(player, player.transform.position);
            }
            if(playerList.Count == 0) return;
            var cachedPlayers = new Dictionary<PlayerControllerB, Vector3>(playerList);
            foreach (var player in cachedPlayers)
            {
                
                var entry = playerList.ElementAt(Random.Range(0, playerList.Count));
                playerIDs.Add(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player.Key));
                playerPositions.Add(entry.Value);
                inside.Add(entry.Key.isInsideFactory);
                playerList.Remove(entry.Key);
            }
            Networker.Instance.swapAllPlayersClientRpc(playerIDs.ToArray(), playerPositions.ToArray(), inside.ToArray());
        }

        public static void FixedUpdate()
        {
            if(!doingStuff) return;
            if (doNext <= 0)
            {
                if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase ||
                    StartOfRound.Instance.shipIsLeaving)
                {
                    doingStuff = false;
                    return;
                }
                doNext = Random.Range(40, 70);
                SwapPlayers();
            }
            doNext -= Time.fixedDeltaTime;
        }
    }
}
