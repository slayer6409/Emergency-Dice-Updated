using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class TulipBombers : IEffect
    {
        public string Name => "Tulip Bombers";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Tulip Bombers";

        public void Use()
        {
           Networker.Instance.doTulipBomberServerRPC(2);
        }

        private static List<Vector3> allPositions = new List<Vector3>();
        public static void spawnAttachedEnemyCombo(int MinesToSpawn, string spawnableEnemy, string spawnableTrap, bool isOutside, Vector3 trapOffset = default, float positionOffsetRadius = 5f, Vector3 sizeOfTrap = default, bool matchSize = false, bool targetRandom = false)
        {
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

            List<GameObject> spawnPoints = RoundManager.Instance.outsideAINodes.ToList();
            if(!isOutside) spawnPoints = RoundManager.Instance.insideAINodes.ToList();
            int totalSpawnPoints = spawnPoints.Count;

            if (totalSpawnPoints == 0)
            {
                return;
            }
            var enemy =  GetEnemies.allEnemies.ToList().Where(x=>x.enemyName==spawnableEnemy).First();
            var trap = Misc.getAllTraps().Where(x => x.name == spawnableTrap).First();

            int maxAttempts = 100;
            while (spawnedMines < MinesToSpawn)
            {
                for (int i = 0; i < totalSpawnPoints && spawnedMines < MinesToSpawn; i++)
                {
                    Vector3 pos = spawnPoints[Random.Range(0, totalSpawnPoints)].transform.position;
                    bool validPositionFound = false;
                    for (int attempt = 0; attempt < maxAttempts && !validPositionFound; attempt++)
                    {
                        Vector3 offset = new Vector3(
                            (float)(random.NextDouble() * 2 - 1) * positionOffsetRadius,
                            0,
                            (float)(random.NextDouble() * 2 - 1) * positionOffsetRadius);

                        Vector3 randomPosition = pos + offset;
                        if (Physics.Raycast(randomPosition + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f))
                        {
                            Vector3 groundPosition = hit.point;
                            if (GetShortestDistanceSqr(groundPosition, allPositions) >= 1)
                            {
                                validPositionFound = true;

                                GameObject gameObject2 = UnityEngine.Object.Instantiate(
                                    enemy.enemyPrefab,
                                    groundPosition,
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);
                                var trapPos = gameObject2.transform.position;
                                if (trapOffset != default)
                                {
                                    trapPos += trapOffset;
                                }
                                GameObject gameObject = UnityEngine.Object.Instantiate(
                                    //CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.Crate.WoodenCratePrefab,
                                    trap.prefab,
                                    trapPos,
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);

                                allPositions.Add(groundPosition);
                                var netobj = gameObject.GetComponent<NetworkObject>();
                                var netobj2 = gameObject2.GetComponent<NetworkObject>();
                                netobj.Spawn(destroyWithScene: true);
                                netobj2.Spawn(destroyWithScene: true);
                                RoundManager.Instance.SpawnedEnemies.Add(gameObject2.GetComponent<EnemyAI>());
                                if (sizeOfTrap != default || matchSize)
                                {
                                    if (matchSize)
                                    {
                                        Networker.Instance.MatchSizeClientRPC(netobj.NetworkObjectId, netobj2.NetworkObjectId);
                                    }
                                    else
                                    {
                                        Networker.Instance.setSizeClientRPC(netobj.NetworkObjectId, sizeOfTrap);
                                    }
                                }

                                if (targetRandom)
                                {
                                    gameObject2.AddComponent<TargetRandomPlayer>();
                                }
                                gameObject.transform.SetParent(gameObject2.transform);
                                spawnedMines++;
                                Networker.Instance.ScanSphereDisableClientRPC(netobj.NetworkObjectId);
                            }
                        }
                    }

                    if (!validPositionFound)
                    {
                        MysteryDice.CustomLogger.LogWarning(
                            "Could not find a valid position for mine at spawn point: " + pos);
                    }
                }
            }
        }
        public static float GetShortestDistanceSqr(Vector3 position, List<Vector3> positions)
        {
            float shortestLength = float.MaxValue;
            foreach (Vector3 pos in positions)
            {
                float distance = (position - pos).sqrMagnitude;
                if (distance < shortestLength)
                    shortestLength = distance;
            }
            return shortestLength;
        }
    }
}
