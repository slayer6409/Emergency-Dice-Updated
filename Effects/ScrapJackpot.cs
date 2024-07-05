using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ScrapJackpot : IEffect
    {
        public string Name => "Scrap jackpot";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Spawning more scrap!";

        public void Use()
        {
            Networker.Instance.JackpotServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, UnityEngine.Random.Range(3, 9));
        }

        public static void JackpotScrap(ulong userID, int amount)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);

            RoundManager RM = RoundManager.Instance;
            List<Item> scrapToSpawn = new List<Item>();

            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            int amountOfScrap = amount;

            List<int> weightList = new List<int>(RM.currentLevel.spawnableScrap.Count);
            for (int j = 0; j < RM.currentLevel.spawnableScrap.Count; j++)
            {
                weightList.Add(RM.currentLevel.spawnableScrap[j].rarity);
            }
            int[] weights = weightList.ToArray();

            for (int i = 0; i < amountOfScrap; i++)
            {
                scrapToSpawn.Add(RoundManager.Instance.currentLevel.spawnableScrap[RM.GetRandomWeightedIndex(weights)].spawnableItem);
            }

            foreach(Item scrap in scrapToSpawn)
            {
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
            }

            RM.StartCoroutine(DelayedSync(RM, netObjs.ToArray(), scrapValues.ToArray(), scrapWeights.ToArray()));
        }
        public static IEnumerator DelayedSync(RoundManager RM, NetworkObjectReference[] netObjs, int[] scrapValues, float[] scrapWeights)
        {
            yield return new WaitForSeconds(3f);
            RM.SyncScrapValuesClientRpc(netObjs, scrapValues);
            Networker.Instance.SyncItemWeightsClientRPC(netObjs, scrapWeights);
        }

    }
}
