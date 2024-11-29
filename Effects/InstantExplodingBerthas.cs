using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class InstantExplodingBerthas : IEffect
    {
        public string Name => "IEBertha";
        public static int MinMinesToSpawn = 2;
        public static int MaxMinesToSpawn = 5;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You might die";
        public void Use()
        {
            Networker.Instance.InstantExplodeBerthaServerRPC();
        }

        public static void SpawnBerthaOutside(int MinesToSpawn, float positionOffsetRadius = 5f)
        {
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

            // Get all spawn points
            List<GameObject> spawnPoints = RoundManager.Instance.outsideAINodes.ToList();
            int totalSpawnPoints = spawnPoints.Count;

            if (totalSpawnPoints == 0)
            {
                return;
            }
            int maxAttempts = 100; 
            while (spawnedMines < MinesToSpawn)
            {
                for (int i = 0; i < totalSpawnPoints && spawnedMines < MinesToSpawn; i++)
                {
                    Vector3 pos = spawnPoints[Random.Range(0,totalSpawnPoints)].transform.position;
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

                                GameObject gameObject = UnityEngine.Object.Instantiate(
                                    GetEnemies.Bertha.prefabToSpawn,
                                    groundPosition,
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);

                                allPositions.Add(groundPosition);
                                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                                gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                                spawnedMines++;
                                gameObject.GetComponentInChildren<Bertha>().ExplodeMineServerRpc(0);

                            }
                        }
                    }

                    if (!validPositionFound)
                    {
                        Debug.LogWarning("Could not find a valid position for mine at spawn point: " + pos);
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
