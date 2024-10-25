using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class AnythingGrenade : IEffect
    {
        public string Name => "Anything Grenade";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "It's not what it seems";

        public void Use()
        {
            Networker.Instance.AnythingGrenadeServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
        public static void Grenade(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);

            Item stunGrenade = Misc.GetItemByName("Easter egg", false);
            if (stunGrenade == null || stunGrenade.spawnPrefab == null)
            {
                Debug.LogError("Stun Grenade prefab not found.");
                return;
            }

            RoundManager RM = RoundManager.Instance;
            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            var scrapItem = RoundManager.Instance.currentLevel.spawnableScrap[UnityEngine.Random.Range(0, RoundManager.Instance.currentLevel.spawnableScrap.Count)];
            var scrap = scrapItem.spawnableItem;

            GameObject obj = UnityEngine.Object.Instantiate(scrap.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
            GrabbableObject component = obj.GetComponent<GrabbableObject>();

            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 0f;
            component.scrapValue = (int)(UnityEngine.Random.Range(scrap.minValue, scrap.maxValue) * RM.scrapValueMultiplier);

            scrapValues.Add(component.scrapValue);
            scrapWeights.Add(component.itemProperties.weight);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            component.FallToGround(true);
            netObjs.Add(netObj);

            if (!obj.TryGetComponent<StunGrenadeItem>(out var newStunGrenadeItem))
            {
                newStunGrenadeItem = obj.AddComponent<StunGrenadeItem>();
            }

            var stunGrenadeItemClone = stunGrenade.spawnPrefab.GetComponent<StunGrenadeItem>();
            if (stunGrenadeItemClone != null)
            {
                CopyStunGrenadeValues(stunGrenadeItemClone, newStunGrenadeItem);
            }

            RM.StartCoroutine(DelayedSync(RM, netObjs.ToArray(), scrapValues.ToArray(), scrapWeights.ToArray()));
        }

        public static void CopyStunGrenadeValues(StunGrenadeItem source, StunGrenadeItem destination)
        {
            var type = typeof(StunGrenadeItem);
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                field.SetValue(destination, field.GetValue(source));
            }
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.CanWrite)
                {
                    property.SetValue(destination, property.GetValue(source, null), null);
                }
            }
        }
        public static IEnumerator DelayedSync(RoundManager RM, NetworkObjectReference[] netObjs, int[] scrapValues, float[] scrapWeights)
        {
            yield return new WaitForSeconds(3f);
            RM.SyncScrapValuesClientRpc(netObjs, scrapValues);
            Networker.Instance.SyncItemWeightsClientRPC(netObjs, scrapWeights);
        }
    }
}
