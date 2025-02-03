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
        public static void SpawnItem(ulong playerID, Vector3 position = default(Vector3))
        {
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            int i = UnityEngine.Random.Range(0, terminal.buyableItemsList.Count());
            Item item = terminal.buyableItemsList[i];

            var posToSpawn = Misc.GetPlayerByUserID(playerID).transform.position;
            if (position != default(Vector3)) posToSpawn = position;
            GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab,
            posToSpawn,
            Quaternion.identity,
            RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();
        }
        public static void SpawnItemNamed(ulong playerID, string name, Vector3 position = default(Vector3))
        {
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            int i = UnityEngine.Random.Range(0, terminal.buyableItemsList.Count());
            
            Item item = terminal.buyableItemsList.ToList().Where(x=>x.itemName==name).First();

            var posToSpawn = Misc.GetPlayerByUserID(playerID).transform.position;
            if(position != default(Vector3)) posToSpawn = position;
            GameObject obj = UnityEngine.Object.Instantiate(item.spawnPrefab,
            posToSpawn,
            Quaternion.identity,
            RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}
