﻿using GameNetcodeStuff;
using LCTarrotCard.Util;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AllSameScrap : IEffect
    {
        public string Name => "Same Scrap Different Dice";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "A bunch of this?!?";
        public void Use()
        {
            RoundManager RM = RoundManager.Instance;
            List<int> weightList = new List<int>(RM.currentLevel.spawnableScrap.Count);
            for (int j = 0; j < RM.currentLevel.spawnableScrap.Count; j++)
            {
                weightList.Add(RM.currentLevel.spawnableScrap[j].rarity);
            }
            int[] weights = weightList.ToArray();
            var item = RM.currentLevel.spawnableScrap[RM.GetRandomWeightedIndex(weights)].spawnableItem;
            Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, UnityEngine.Random.Range(3, 5), item.itemName);
        }

        public static void SameScrap(ulong userID, int amount, string scrapSpawn, bool sneaky=false)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            RoundManager RM = RoundManager.Instance;

            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            int amountOfScrap = amount;
                var item = Misc.GetItemByName(scrapSpawn,false);
            if (scrapSpawn == "takey") item = Misc.GetItemByName("Smol Takey", false);
            if (item == null)
            {
                if(scrapSpawn != "plushies")
                {
                    if (scrapSpawn != "takey")
                    {
                        MysteryDice.CustomLogger.LogError($"No item found with name {scrapSpawn}");
                        return;
                    }
                }
            }
            List<Item> plushies = new List<Item>();
            if (scrapSpawn == "plushies")
            {
                foreach (Item items in StartOfRound.Instance.allItemsList.itemsList)
                {
                    if (items.itemName.ToUpper().Contains("PLUSH"))
                    {
                        if (items.spawnPrefab != null)
                        {
                            plushies.Add(items);
                        }
                    }
                }
            }
            if (scrapSpawn == "takey")
            {
                foreach (Item items in StartOfRound.Instance.allItemsList.itemsList)
                {
                    if (items.itemName.ToUpper().Contains("TAKEY")&& (items.itemName.ToUpper().Contains("PLUSH") || items.itemName.ToUpper().Contains("SMOL") || items.itemName.ToUpper().Contains("GOKU")))
                    {
                        if (items.spawnPrefab != null)
                        {
                            plushies.Add(items);
                        }
                    }
                }
            }
            if (plushies.Count == 0) 
            {
                MysteryDice.CustomLogger.LogError($"No Items in the list with {scrapSpawn} as the name");
                return;
            }
            plushies = plushies.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < amountOfScrap; i++)
            {
                if (scrapSpawn == "plushies"||scrapSpawn=="takey")
                {
                    item = plushies[i];
                }
                //EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(-100,100),
                    UnityEngine.Random.Range(-100,100),
                    UnityEngine.Random.Range(-100,100)
                );
                Ray ray = new Ray(randomPosition, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    randomPosition = hit.point;
                }

                GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, randomPosition, Quaternion.identity, RM.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                component.scrapValue = (int)(UnityEngine.Random.Range(item.minValue, item.maxValue) * RM.scrapValueMultiplier);
                scrapValues.Add(component.scrapValue);
                scrapWeights.Add(component.itemProperties.weight);

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                component.FallToGround(true);
                netObjs.Add(netObj);
            }

            RM.StartCoroutine(DelayedSyncAndTeleport(RM, netObjs.ToArray(), scrapValues.ToArray(), scrapWeights.ToArray(), player));
        }
        
        public static IEnumerator DelayedSyncAndTeleport(RoundManager RM, NetworkObjectReference[] netObjs, int[] scrapValues, float[] scrapWeights, PlayerControllerB player)
        {
            yield return new WaitForSeconds(1f);
            Networker.Instance.AllOfOneTPServerRPC(netObjs, player.playerClientId);
            yield return new WaitForSeconds(2f);
            RM.SyncScrapValuesClientRpc(netObjs, scrapValues);
            Networker.Instance.SyncItemWeightsClientRPC(netObjs, scrapWeights);
        }
        public static void teleport(NetworkObjectReference[] netObjs, ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            foreach (var netObjRef in netObjs)
            {
                if (netObjRef.TryGet(out NetworkObject netObj))
                {
                    GameObject obj = netObj.gameObject;
                    if (obj == null)
                    {
                        continue;
                    }
                    Vector3 targetPosition = player.transform.position + new Vector3(0, -.5f, 0);
                    RaycastHit hit;
                    if (Physics.Raycast(targetPosition + Vector3.up, Vector3.down, out hit))
                    {
                        targetPosition = hit.point;
                    }
                    obj.transform.position = targetPosition;
                    GrabbableObject grabbableObject = obj.GetComponent<GrabbableObject>();
                    if (grabbableObject != null)
                    {
                        grabbableObject.targetFloorPosition = targetPosition;
                    }
                }
            }
        }
    }
}