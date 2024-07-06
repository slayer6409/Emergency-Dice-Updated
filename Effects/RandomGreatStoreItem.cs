using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class RandomGreatStoreItem : IEffect
    {
        public string Name => "Random Store Items";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Many random store items for you!";
        public void Use()
        {
            Networker.Instance.RandomStoreItemsRpc(GameNetworkManager.Instance.localPlayerController.playerClientId, UnityEngine.Random.Range(2,5));
        }
        public static void SpawnItem(ulong playerID, List<Item> items)
        {
            foreach (Item item in items) 
            {
                GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab,
                Misc.GetPlayerByUserID(playerID).transform.position,
                Quaternion.identity,
                RoundManager.Instance.playersManager.propsContainer);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                obj.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
