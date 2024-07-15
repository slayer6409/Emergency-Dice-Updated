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
    internal class RandomStoreItem : IEffect
    {
        public string Name => "Random Store Item";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "A random store item for you!";
        public void Use()
        {
            Networker.Instance.RandomStoreItemServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
        public static void SpawnItem(ulong playerID)
        {
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            int i = UnityEngine.Random.Range(0, terminal.buyableItemsList.Count());
            Item item = terminal.buyableItemsList[i];
            
            GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab,
            Misc.GetPlayerByUserID(playerID).transform.position,
            Quaternion.identity,
            RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}
