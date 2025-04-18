using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TarotCards : IEffect
    {
        public string Name => "Tarot Cards";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "A different kind of gambling!";

        public void Use()
        {
            Networker.Instance.TarotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }
        public static void TarotScrap(int userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);

            List<Item> items = UnityEngine.Resources.FindObjectsOfTypeAll<Item>().ToList();
            Item TarotCardsItem = items.LastOrDefault(item => item.name.Equals("TarrotCard") && item.spawnPrefab != null);
            RoundManager RM = RoundManager.Instance;
            List<Item> scrapToSpawn = new List<Item>();
            //tarrot_item_prefab(Clone) with prefab name tarrot_item_prefab, and properties name TarrotCard + Tarrot Card
            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            scrapToSpawn.Add(TarotCardsItem);
            
            foreach (Item scrap in scrapToSpawn)
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
                obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
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
