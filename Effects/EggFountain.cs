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
            Networker.Instance.EggFountainServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController), 0);
        }

        public static void spawnEggs(int callerID, int choice)
        {
            Item item = null;
            var egg = Misc.GetItemByName("Easter egg", false);
            var stun = Misc.GetItemByName("Stun grenade", false);
            int numberOfItems = 12;
            float radius = 5f;
            int count = 0;
            List<GameObject> spawnedItems = new List<GameObject>();
            List<NetworkObject> spawnedNetItems = new List<NetworkObject>();
            List<Vector3> spawnedPoints = new List<Vector3>();
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

            PlayerControllerB player = Misc.GetPlayerByUserID(callerID); 

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

                spawnedPoints.Add( spawnPosition );
                GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                component.targetFloorPosition = spawnPosition;
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
                obj.GetComponent<GrabbableObject>().EnablePhysics(true);
                component.FallToGround(true);

                spawnedItems.Add(obj);
                spawnedNetItems.Add(netObj);
               

            }
            for (int i = 0; i < spawnedNetItems.Count; i++)
            {
                NetworkObject net = spawnedNetItems[i];
                Vector3 spawnPos = spawnedPoints[i];
                spawnPos.y=player.transform.position.y;
                Networker.Instance.TeleportEggServerRPC(net, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), spawnPos);

                Networker.Instance.explodeItemServerRPC(net.NetworkObjectId, useEgg, count);
                count += 1;
            }
            //explodeStuff(spawnedNetItems, useEgg);
        }
        public static void teleport(NetworkObjectReference netObjs, int userID, Vector3 pos)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (netObjs.TryGet(out NetworkObject netObj))
            {
                GameObject obj = netObj.gameObject;
                if (obj == null) return;
                
                Vector3 targetPosition = pos;
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
        public static IEnumerator explodeEgg(GameObject obj, int count)
        {
            // Wait for the calculated delay based on the count
            yield return new WaitForSeconds(count * MysteryDice.eggExplodeTime.Value);
            
            // Ensure the StunGrenadeItem component exists
            var stunGrenadeItem = obj.GetComponent<StunGrenadeItem>();
            if (stunGrenadeItem != null)
            {
                stunGrenadeItem.chanceToExplode = 100;

                // Use reflection to invoke the private method
                var explodeMethod = AccessTools.Method(stunGrenadeItem.GetType(), "ExplodeStunGrenade");
                if (explodeMethod != null)
                {
                    try
                    {
                        explodeMethod.Invoke(stunGrenadeItem, new object[] { true });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to invoke ExplodeStunGrenade: {ex}");
                    }
                }
                else
                {
                    Debug.LogError("ExplodeStunGrenade method not found.");
                }
            }
            else
            {
                Debug.LogError("StunGrenadeItem component missing on the object.");
            }

            // Reset the spawn flag
            EggBoots.canSpawnAnother = true;
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
            Networker.Instance.EggFountainServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController), 1);
        }
    }
}
