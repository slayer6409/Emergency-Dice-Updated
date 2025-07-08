using System;
using GameNetcodeStuff;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class EggBoots : IEffect
    {
        public string Name => "Egg Boots";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Rocket Jump!";
        
        public static bool eggBootsEnabled = false;

        public static bool canSpawnAnother = true;
        public void Use()
        {
            Networker.Instance.EggBootsServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }
        public static void SpawnAndExplodeEgg(int playerid)
        {

            var item = Misc.GetItemByName("Easter egg", false);
            var player = Misc.GetPlayerByUserID(playerid);
            Vector3 spawnPosition = player.transform.position - player.transform.forward * 3.5f; // 5 is the distance behind
            spawnPosition.y += 0.25f;

            GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
            GameObject obj2 = UnityEngine.Object.Instantiate(item.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
            GrabbableObject component = obj.GetComponent<GrabbableObject>();
            GrabbableObject component2 = obj2.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component2.transform.rotation = Quaternion.Euler(component2.itemProperties.restingRotation);
            component.fallTime = 10f;
            component2.fallTime = 10f;
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            NetworkObject netObj2 = obj2.GetComponent<NetworkObject>();
            netObj.Spawn();
            netObj2.Spawn(); 
            Vector3 sps = new Vector3(obj.transform.position.x, spawnPosition.y, obj.transform.position.z);
            component.targetFloorPosition = sps;
            component2.targetFloorPosition = sps;
            component.FallToGround(false, true, spawnPosition);
            component2.FallToGround(false, true, spawnPosition);
            Networker.Instance.explodeItemServerRPC(netObj.NetworkObjectId, true, 0);
            Networker.Instance.explodeItemServerRPC(netObj2.NetworkObjectId, true, 0);
        }
    }
    public class EggNoKill : MonoBehaviour
    {
        public bool eggKill = false;
    }
}
