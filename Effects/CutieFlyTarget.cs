using MysteryDice.Patches;
using System;
using GameNetcodeStuff;
using MysteryDice;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class HeatSeakingCutieFly : IEffect
    {
        public string Name => "Heat Seaking Cutie Fly";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Oh God";

        public void Use()
        {
            Networker.Instance.doEvilCutieFlyServerRPC();
        }

        public static void doSpawnCutie()
        {
            var allPos = RoundManager.Instance.outsideAINodes;
            for (int i = 0; i < 6; i++)
            {
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    GetEnemies.allEnemies.Find(x=>x.enemyName=="CutieFly").enemyPrefab,
                    allPos[Random.Range(0, allPos.Length)].transform.position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
                enemyObject.AddComponent<TargetRandomPlayer>();
            }
        }
    }
}

public class TargetRandomPlayer : MonoBehaviour
{
    NavMeshAgent agent;
    private PlayerControllerB target;
    public void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        target = Misc.GetRandomAlivePlayer();
    }

    public void Update()
    {
        if(target == null||target.isPlayerDead) target = Misc.GetRandomAlivePlayer();
        agent.SetDestination(target.transform.position);
        
    }
}
