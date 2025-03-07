using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class WhereDidMyFriendsGo : IEffect
    {
        public string Name => "Where Did My Friends Go?";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Where did they go?";
        
        public static List<PlayerControllerB> hiddenPlayers = new List<PlayerControllerB>();

        public void Use()
        {
            Networker.Instance.WhereGoServerRPC(Misc.GetRandomPlayerID());
        }
        public static void TurnPlayerInvisible(PlayerControllerB player, bool value)
        {
            GameObject scavengerModel = player.gameObject.transform.Find("ScavengerModel").gameObject;
            if (scavengerModel == null) { MysteryDice.CustomLogger.LogError("ScavengerModel not found"); return; }
            scavengerModel.transform.Find("LOD1").gameObject.SetActive(!value);
            scavengerModel.transform.Find("LOD2").gameObject.SetActive(!value);
            scavengerModel.transform.Find("LOD3").gameObject.SetActive(!value);
            player.playerBadgeMesh.gameObject.SetActive(value);
        }
        public static void whereTheyGo(ulong userID)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            if (StartOfRound.Instance.localPlayerController.actualClientId != userID) return;
            var ipc = StartOfRound.Instance.localPlayerController.gameObject.AddComponent<invisiblePlayerController>();
            foreach (var playerControllerB in StartOfRound.Instance.allPlayerScripts)
            {
                if(playerControllerB==StartOfRound.Instance.localPlayerController) continue;
                if(playerControllerB.isPlayerControlled || playerControllerB.isPlayerDead)
                {
                    ipc.HiddenPlayers.Add(playerControllerB);
                }
            }
        }

    }

    public class invisiblePlayerController : MonoBehaviour
    {
        // public Dictionary<PlayerControllerB, (GameObject,GameObject,GameObject)> playersLODs = new Dictionary<PlayerControllerB, (GameObject, GameObject, GameObject)>();
        public List<PlayerControllerB> HiddenPlayers = new List<PlayerControllerB>();
        public void FixedUpdate()
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded)
            {
                fixStuff();
                return;
            }
            
            foreach (var player in HiddenPlayers)
            {
                if(player==StartOfRound.Instance.localPlayerController)continue;
                player.thisPlayerModelLOD1.gameObject.SetActive(false);
                player.thisPlayerModelLOD2.gameObject.SetActive(false);
                player.thisPlayerModel.gameObject.SetActive(false);
                player.playerBadgeMesh.gameObject.SetActive(false);
                player.playerBetaBadgeMesh.gameObject.SetActive(false);
                if (MysteryDice.MoreCompanyPresent)
                {
                    WhereDidMyFriendsGoPt2.ToggleCosmetics(player.actualClientId, false);
                }
            }
        }

        public void fixStuff()
        {
            foreach (var player in HiddenPlayers)
            {
                player.thisPlayerModelLOD1.gameObject.SetActive(true);
                player.thisPlayerModelLOD2.gameObject.SetActive(true);
                player.thisPlayerModel.gameObject.SetActive(true);
                player.playerBadgeMesh.gameObject.SetActive(true);
                player.playerBetaBadgeMesh.gameObject.SetActive(true);
                if (MysteryDice.MoreCompanyPresent)
                {
                    WhereDidMyFriendsGoPt2.ToggleCosmetics(player.actualClientId, true);
                }
            }
            Destroy(this);
        }

    }
}
