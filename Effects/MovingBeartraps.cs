using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using System.Reflection;
using CodeRebirth.src.Content.Maps;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MovingBeartraps : IEffect
    {
        public string Name => "Roomba Beartraps";

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Roomba Beartraps?";

        private static List<GameObject> bearTraps = new List<GameObject>();
        public void Use()
        {
            Networker.Instance.MovingBeartrapsServerRPC();
        }

        // public static void spawnBeartraps(bool indoors = false)
        // {
        //     
        //         bearTraps.Add(mapObjectHandlerInstance.BearTrap.GrassMatPrefab);
        //         bearTraps.Add(mapObjectHandlerInstance.BearTrap.GravelMatPrefab);
        //         bearTraps.Add(mapObjectHandlerInstance.BearTrap.SnowMatPrefab);
        //     
        //     
        //
        //     if (bearTraps.Count == 0)
        //     {
        //         Debug.LogError("bearTraps list is empty. Ensure BearTrap prefabs are properly assigned in MapObjectHandler.");
        //         return;
        //     }
        //
        //     var spawnPoints = indoors ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes;
        //     if (spawnPoints == null || spawnPoints.Length == 0)
        //     {
        //         Debug.LogError("Spawn points array is empty. Ensure RoundManager has valid AI nodes set up.");
        //         return;
        //     }
        //
        //     for (int i = 0; i <= 10; i++)
        //     {
        //         var spawnPrefab = bearTraps[Random.Range(0, bearTraps.Count)];
        //         var randomNodeIndex = Random.Range(0, spawnPoints.Length);
        //         var randomPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(spawnPoints[randomNodeIndex].transform.position);
        //
        //         var bt = GameObject.Instantiate(spawnPrefab, randomPosition, Quaternion.identity);
        //         bt.GetComponent<NetworkObject>().Spawn();
        //     }
        // }
     }
}
