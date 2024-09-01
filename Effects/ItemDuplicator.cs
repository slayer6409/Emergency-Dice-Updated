using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
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
            Networker.Instance.ItemDuplicatorServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void duplicateItems(ulong userID)
        {
            PlayerControllerB player = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp.playerClientId == userID)
                {
                    player = playerComp;
                    break;
                }
            }
            if (player == null) return;
            foreach (var i in player.ItemSlots)
            {
                if (i == null) continue;
                GameObject obj = UnityEngine.Object.Instantiate(i.itemProperties.spawnPrefab, player.transform.position, Quaternion.identity,RoundManager.Instance.playersManager.propsContainer);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                obj.GetComponent<GrabbableObject>().scrapValue = i.scrapValue;
                obj.GetComponent<GrabbableObject>().itemProperties.weight  = i.itemProperties.weight;
                obj.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
