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
            Networker.Instance.RandomStoreItemsServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId, UnityEngine.Random.Range(2,5));
        }
        public static void SpawnItem(ulong playerID, int icount)
        {
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            List<Item> items = new List<Item>();
            for (int i = 0; i < icount; i++)
            {
                int e = UnityEngine.Random.Range(0, terminal.buyableItemsList.Count());
                items.Add(terminal.buyableItemsList[e]);
            }
            foreach (Item item in items) 
            {
                GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab,
                Misc.GetPlayerByUserID(playerID).transform.position,
                Quaternion.identity,
                RoundManager.Instance.playersManager.propsContainer);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                obj.GetComponent<NetworkObject>().Spawn();
                obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            }
        }
    }
}
