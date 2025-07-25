using MysteryDice.Patches;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class EvilFreebirdEnemy : IEffect
    {
        public string Name => "Evil Freebird Enemy";
        public EffectType Outcome => EffectType.GalMixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Freebird that has a higher chance to target players";
        
        public void Use()
        {
            Networker.Instance.SpawnFreebirdEnemyServerRPC();
        }

        public static void spawnEnemy()
        {
            var enemy = GetEnemies.allEnemies[UnityEngine.Random.Range(0, GetEnemies.allEnemies.Count)];
            var RM = RoundManager.Instance;
            
            Vector3 position = Vector3.zero;
            
            EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
            
            if (enemy.isOutsideEnemy || enemy.isDaytimeEnemy)
            {
                position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            }
            else
            {
                position = randomVent.floorNode.position;
            }
            GameObject gameObject = UnityEngine.Object.Instantiate(
                enemy.enemyPrefab,
                position,
                Quaternion.identity);
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
            var netobj = gameObject.GetComponent<NetworkObject>();
            netobj.Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            Networker.Instance.FreebirdEnemyServerRPC(netobj.NetworkObjectId);
        }
        

        public static void fixEnemy(ulong id)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var e = obj.gameObject.AddComponent<freebirdMaker>();
                e.isEvil = true;
            }
        }
        
        public static void fixTrap(ulong id)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                obj.gameObject.AddComponent<freebirdTrapMaker>();
                if (MysteryDice.CodeRebirthPresent)
                {
                    Networker.Instance.StartCoroutine(MovingBeartraps.fixSpiney(obj));
                }
            }
        }
    }
}
