using GameNetcodeStuff;
using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EggFountain : IEffect
    {
        public string Name => "Surprise Eggs!!!";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Pop goes the egg!";

        public void Use()
        {
            Networker.Instance.EggFountainServerRPC(StartOfRound.Instance.localPlayerController.playerClientId, 0);
        }

        public static void spawnEggs(ulong callerID, int choice)
        {
            Item item = null;
            var egg = Misc.GetItemByName("Easter egg", false);
            var stun = Misc.GetItemByName("Stun grenade", false);
            int numberOfItems = 12;
            float radius = 5f;
            int count = 0;
            List<GameObject> spawnedItems = new List<GameObject>();
            List<NetworkObject> spawnedNetItems = new List<NetworkObject>();
            bool useEgg = false;

            switch (choice)
            {
                case 0:
                    item = egg;
                    useEgg = true;
                    break;
                case 1:
                    item = stun;
                    radius = 3f;
                    break;
                default:
                    item = egg;
                    useEgg = true;
                    break;
            }

            PlayerControllerB player = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp == null) continue;

                if (playerComp.playerClientId == callerID)
                {
                    player = playerComp;
                }
                if (player != null)
                {
                    break;
                }
            }

            if (player == null) return;

            for (int i = 0; i < numberOfItems; i++)
            {
                float angle = i * Mathf.PI * 2 / numberOfItems;
                Vector3 spawnPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    player.transform.position.y + 0.25f,
                    Mathf.Sin(angle) * radius
                );
                spawnPosition += player.transform.position;

                GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                component.scrapValue = (int)(UnityEngine.Random.Range(item.minValue, item.maxValue) * RoundManager.Instance.scrapValueMultiplier);
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                component.FallToGround(true);

                spawnedItems.Add(obj);
                spawnedNetItems.Add(netObj);
               
                Networker.Instance.explodeItemServerRPC(netObj.NetworkObjectId, useEgg, count);
                count += 1;
            }
            //explodeStuff(spawnedNetItems, useEgg);
        }
        public static IEnumerator explodeEgg(GameObject obj, int count)
        {
            yield return new WaitForSeconds(count*MysteryDice.eggExplodeTime.Value);
            obj.GetComponent<StunGrenadeItem>().chanceToExplode = 100;
            AccessTools.Method(obj.GetComponent<StunGrenadeItem>().GetType(), "ExplodeStunGrenade")
                ?.Invoke(obj.GetComponent<StunGrenadeItem>(), new object[] { true });
            EggBoots.canSpawnAnother=true;
        }

        public static IEnumerator explodeStun(GameObject obj)
        {
            yield return new WaitForSeconds(1f);
            obj.GetComponent<StunGrenadeItem>().chanceToExplode = 100;
            AccessTools.Method(obj.GetComponent<StunGrenadeItem>().GetType(), "ExplodeStunGrenade")
                ?.Invoke(obj.GetComponent<StunGrenadeItem>(), new object[] { true });

            yield return new WaitForSeconds(0.0f);
        }
    }

    [HarmonyPatch(typeof(StunGrenadeItem), "ExplodeStunGrenade")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class StunGrenadePatch
    {
        [HarmonyPostfix]
        public static void PostfixExplodeStunGrenade(bool __state)
        {
            MysteryDice.CustomLogger.LogDebug("Stun grenade exploded");
        }
    }
    internal class FlashFountain : IEffect
    {
        public string Name => "Surprise Flash!!!";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Flashbang inbound!";
        public void Use()
        {
            Networker.Instance.EggFountainServerRPC(StartOfRound.Instance.localPlayerController.playerClientId, 1);
        }
    }
}
