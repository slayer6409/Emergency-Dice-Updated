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
            Networker.Instance.RandomStoreItemServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }
        public static void SpawnItem(int playerID, Vector3 position = default(Vector3))
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
            CullFactorySoftCompat.RefreshGrabbableObjectPosition(obj.GetComponent<GrabbableObject>());
        }
        public static void SpawnItemNamed(int playerID, string name, Vector3 position = default(Vector3))
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
            CullFactorySoftCompat.RefreshGrabbableObjectPosition(obj.GetComponent<GrabbableObject>());
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
        }
    }
}
