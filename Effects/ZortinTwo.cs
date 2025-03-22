using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ZortinTwo : IEffect
    {
        public string Name => "Zortin 2";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Zort 2";
        public void Use()
        {
            Networker.Instance.doZortServerRpc(StartOfRound.Instance.localPlayerController.actualClientId);
        }

        public static void spawnZort(ulong playerid)
        { 
            RoundManager RM = RoundManager.Instance;

            var player = Misc.GetPlayerByUserID(playerid);
            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            var item = Misc.GetItemByName("Violin",false);
            GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
            GrabbableObject component = obj.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 0f;
            component.scrapValue = (int)(UnityEngine.Random.Range(item.minValue, item.maxValue) * RM.scrapValueMultiplier);
            scrapValues.Add(component.scrapValue);
            scrapWeights.Add(component.itemProperties.weight);
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();
            obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
            component.FallToGround(true);
            netObjs.Add(netObj);
            
            
            var item2 = Misc.GetItemByName("Accordion",false);
            GameObject obj2 = UnityEngine.Object.Instantiate(item2.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
            GrabbableObject component2 = obj2.GetComponent<GrabbableObject>();
            component2.transform.rotation = Quaternion.Euler(component2.itemProperties.restingRotation);
            component2.fallTime = 0f;
            component2.scrapValue = (int)(UnityEngine.Random.Range(item2.minValue, item2.maxValue) * RM.scrapValueMultiplier);
            scrapValues.Add(component2.scrapValue);
            scrapWeights.Add(component2.itemProperties.weight);
            NetworkObject netObj2 = obj2.GetComponent<NetworkObject>();
            netObj2.Spawn();
            obj2.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
            component2.FallToGround(true);
            netObjs.Add(netObj2);
            
            
            var item3 = Misc.GetItemByName("Guitar",false);
            
            GameObject obj3 = UnityEngine.Object.Instantiate(item3.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
            GrabbableObject component3 = obj3.GetComponent<GrabbableObject>();
            component3.transform.rotation = Quaternion.Euler(component3.itemProperties.restingRotation);
            component3.fallTime = 0f;
            component3.scrapValue = (int)(UnityEngine.Random.Range(item3.minValue, item3.maxValue) * RM.scrapValueMultiplier);
            scrapValues.Add(component3.scrapValue);
            scrapWeights.Add(component3.itemProperties.weight);
            NetworkObject netObj3 = obj3.GetComponent<NetworkObject>();
            netObj3.Spawn();
            obj3.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
            component3.FallToGround(true);
            netObjs.Add(netObj3);
            
            var item4 = Misc.GetItemByName("Recorder",false);
            GameObject obj4 = UnityEngine.Object.Instantiate(item4.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
            GrabbableObject component4 = obj4.GetComponent<GrabbableObject>();
            component4.transform.rotation = Quaternion.Euler(component4.itemProperties.restingRotation);
            component4.fallTime = 0f;
            component4.scrapValue = (int)(UnityEngine.Random.Range(item4.minValue, item4.maxValue) * RM.scrapValueMultiplier);
            scrapValues.Add(component4.scrapValue);
            scrapWeights.Add(component4.itemProperties.weight);
            NetworkObject netObj4 = obj4.GetComponent<NetworkObject>();
            netObj4.Spawn();
            obj4.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
            component4.FallToGround(true);
            netObjs.Add(netObj4);
            
            RM.StartCoroutine(DelaySync(RM, netObjs.ToArray(), scrapValues.ToArray(), scrapWeights.ToArray()));
            Networker.Instance.doZortStuffServerRpc(netObj.NetworkObjectId,netObj4.NetworkObjectId,netObj2.NetworkObjectId, netObj3.NetworkObjectId);
        }

        private static IEnumerator DelaySync(RoundManager RM, NetworkObjectReference[] netObjs, int[] scrapValues, float[] scrapWeights)
        {
            yield return new WaitForSeconds(2f);
            RM.SyncScrapValuesClientRpc(netObjs, scrapValues);
            Networker.Instance.SyncItemWeightsClientRPC(netObjs, scrapWeights);
        }
    }
}
