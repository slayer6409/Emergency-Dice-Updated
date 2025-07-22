using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class FreebirdJimothy : IEffect
    {
        public string Name => "Freebird Jimothy";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "FREEBIRD";
        
    
        public void Use()
        {
            Networker.Instance.SpawnFreebirdJimothyServerRPC();
        }

        public static void spawnJimothy()
        {
            Vector3 position = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position;
            GameObject gameObject = UnityEngine.Object.Instantiate(
                GetEnemies.Jimothy.enemyType.enemyPrefab,
                position,
                Quaternion.identity);
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
            var netobj = gameObject.GetComponent<NetworkObject>();
            netobj.Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            Networker.Instance.FreebirdJimothyServerRPC(netobj.NetworkObjectId);
        }

        public static void fixJimothy(ulong id)
        {
            var speed = 19;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var transporter = obj.GetComponent<CodeRebirth.src.Content.Enemies.Transporter>();
                transporter.gameObject.AddComponent<freebirdMaker>();
                transporter.speedIncrease = speed;
            }
        }
    }
}
