using MysteryDice.Patches;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace MysteryDice.Effects
{
    internal class MicrowaveBertha : IEffect
    {
        public string Name => "Microwavable Bertha";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "MMMMMMMMMMMMMM BOOM";

        public void Use()
        {
            Networker.Instance.MicrowaveBerthaServerRPC(UnityEngine.Random.Range(1, 4));
        }

        public static void spawnMicrowaveBertha(int num)
        {
            for (int i = 0; i < num; i++)
            {
                var enemy = GetEnemies.Microwave;
                var enemy2 = GetEnemies.Bertha;
                RoundManager RM = RoundManager.Instance;

                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
                GameObject[] aiNodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                aiNodes = aiNodes.OrderBy(x => Vector3.Distance(x.transform.position, Vector3.zero)).ToArray();

                Vector3 position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
                position = RM.GetRandomNavMeshPositionInBoxPredictable(position, 30f, default(NavMeshHit), random) + Vector3.up;

                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    enemy.prefabToSpawn,
                    position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                GameObject enemyObject2 = UnityEngine.Object.Instantiate(
                    enemy2.prefabToSpawn,
                    enemyObject.transform.position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                enemyObject2.transform.SetParent(enemyObject.transform);
            }
           
        }
    }
}
