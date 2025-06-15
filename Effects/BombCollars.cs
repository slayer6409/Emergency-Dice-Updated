using BombCollar;
using Discord;
using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace MysteryDice.Effects
{
    internal class BombCollars : IEffect
    {
        public string Name => "Friendship Bracelets";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Forced Friendship!";

        public void Use()
        {
            Networker.Instance.forcedFriendshipServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, StartOfRound.Instance.localPlayerController));
        }
        public static void spawnCollars(int playerID, bool stuck = false, int min = 2, int max = 4)
        {
            //glitch = 76561198984467725
            //me = 76561198077184650
            var glitch = Misc.getPlayerBySteamID(76561198984467725);
            PlayerControllerB player = Misc.GetPlayerByUserID(playerID);
            RoundManager RM = RoundManager.Instance;

            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();
            Item item = Misc.GetItemByName("Bomb collar", false);
            int amountOfScrap = UnityEngine.Random.Range(min, max + 1);
            for (int i = 0; i < amountOfScrap; i++)
            {
                try
                {
                    Vector3 playerPos = player.transform.position + new Vector3(0, .25f, 0);
                    Ray ray = new Ray(playerPos, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        playerPos = hit.point;
                    }

                    GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, playerPos, Quaternion.identity, RM.spawnedScrapContainer);
                    GrabbableObject component = obj.GetComponent<GrabbableObject>();
                    component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                    component.fallTime = 0f;
                    component.scrapValue = (int)(UnityEngine.Random.Range(item.minValue, item.maxValue) * RM.scrapValueMultiplier);
                    scrapValues.Add(component.scrapValue);
                    scrapWeights.Add(component.itemProperties.weight);
                    NetworkObject netObj = obj.GetComponent<NetworkObject>();
                    netObj.Spawn();
                    CullFactorySoftCompat.RefreshGrabbableObjectPosition(obj.GetComponent<GrabbableObject>());
                    obj.GetComponent<GrabbableObject>().EnablePhysics(true);
                    //component.FallToGround(true);
                    EggFountain.teleport(netObj,Array.IndexOf(StartOfRound.Instance.allPlayerScripts, StartOfRound.Instance.localPlayerController),component.transform.position+new Vector3(0,.25f,0));
                    BombCollarProp bcp = obj.GetComponent<BombCollarProp>();
                    BombCollar.BombCollarBase.Instance.AllBombCollars.Add(bcp);
                    netObjs.Add(netObj);
                    if (i == 0)
                    {
                        if (glitch != null)
                        {
                            if (bcp != null)
                                if (Misc.IsPlayerAliveAndControlled(glitch)) 
                                {
                                    bcp.AttachToPlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,glitch));
                                    Networker.Instance.TeleportToPlayerServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,glitch), Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                                } 
                        }
                        else if (stuck)
                        {
                            if (bcp != null)
                                if (Misc.IsPlayerAliveAndControlled(player)) bcp.AttachToPlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        public static void EveryoneIsFriendsNow()
        {
            RoundManager RM = RoundManager.Instance;
            Vector3 pos = StartOfRound.Instance.middleOfShipNode.position + new Vector3(0,.25f,0);
            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();
            Item item = Misc.GetItemByName("Bomb collar", false);
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {

                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (Misc.IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }
            foreach(var player in validPlayers)
            {
                try
                {
                    Vector3 spawnPos = pos;
                    Ray ray = new Ray(pos, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        pos = hit.point;
                    }

                    GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, pos, Quaternion.identity, RM.spawnedScrapContainer);
                    GrabbableObject component = obj.GetComponent<GrabbableObject>();
                    component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                    component.fallTime = 0f;
                    component.scrapValue = (int)(UnityEngine.Random.Range(item.minValue, item.maxValue) * RM.scrapValueMultiplier);
                    scrapValues.Add(component.scrapValue);
                    scrapWeights.Add(component.itemProperties.weight);
                    NetworkObject netObj = obj.GetComponent<NetworkObject>();
                    netObj.Spawn();
                    //component.FallToGround(true);
                    netObjs.Add(netObj);
                    EggFountain.teleport(netObj,Array.IndexOf(StartOfRound.Instance.allPlayerScripts, StartOfRound.Instance.localPlayerController),component.transform.position+new Vector3(0,.25f,0));
                    BombCollarProp bcp = obj.GetComponent<BombCollarProp>();
                    BombCollar.BombCollarBase.Instance.AllBombCollars.Add(bcp);
                    
                    if (obj.GetComponent<BombCollarProp>() != null)
                        if (Misc.IsPlayerAliveAndControlled(player)) bcp.AttachToPlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                        
                    
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            EmergencyMeeting.TeleportEveryoneToShip();
        }

    }
    internal class EveryoneFriends : IEffect
    {
        public string Name => "Everyone Friends";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Everyone is Friends now!";

        public void Use()
        {
            Networker.Instance.everyoneFriendsServerRPC();
        }
    }
}
