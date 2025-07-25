﻿using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ItemDuplicator : IEffect
    {
        public string Name => "Item Duplicator";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Duplicates your inventory on the ground";

        public void Use()
        {
            Networker.Instance.ItemDuplicatorServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public static void duplicateItems(int userID)
        {
            List<Item> scrapToSpawn = new List<Item>();
            RoundManager RM = RoundManager.Instance;
            List<NetworkObjectReference> netObjs = new List<NetworkObjectReference>();
            List<int> scrapValues = new List<int>();
            List<float> scrapWeights = new List<float>();
            PlayerControllerB player = null;

            player = StartOfRound.Instance.allPlayerScripts[userID];
            
            if (player == null) return;
            foreach (var i in player.ItemSlots)
            {
                if (i == null) continue;

                GameObject obj = UnityEngine.Object.Instantiate(i.itemProperties.spawnPrefab, player.transform.position, Quaternion.identity, RM.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                component.scrapValue = i.scrapValue;
                component.itemProperties.weight = i.itemProperties.weight;
                scrapValues.Add(component.scrapValue);
                scrapWeights.Add(component.itemProperties.weight);

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();
                CullFactorySoftCompat.RefreshGrabbableObjectPosition(obj.GetComponent<GrabbableObject>());
                obj.GetComponent<GrabbableObject>().EnablePhysics(true);
                component.FallToGround(false, true, player.transform.position);
                netObjs.Add(netObj);
            }
            RM.StartCoroutine(ScrapJackpot.DelayedSync(RM, netObjs.ToArray(), scrapValues.ToArray(), scrapWeights.ToArray()));
        }
    }
}
