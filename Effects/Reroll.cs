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
    internal class Reroll : IEffect
    {
        public string Name => "Reroll";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Try Again";

        public void Use()
        {
            Networker.Instance.RerollServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }
        public static void DiceScrap(int userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);

            RoundManager RM = RoundManager.Instance;
            List<Item> scrapToSpawn = new List<Item>();

            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();

            scrapToSpawn.Add(MysteryDice.RegisteredDice[UnityEngine.Random.Range(0,MysteryDice.RegisteredDice.Count)]);
            
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
                component.FallToGround(false, true, player.transform.position);
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
