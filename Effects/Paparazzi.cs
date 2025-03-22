using System;
using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using MysteryDice.Patches;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Paparazzi : IEffect
    {
        public string Name => "Paparazzi";
        public static int MinMinesToSpawn = 3;
        public static int MaxMinesToSpawn = 6;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Papa-paparazzi";

        public void Use()
        {
            Networker.Instance.doPaparazziServerRPC();
        }
        public static void SpawnPaparazzi(int MinesToSpawn, float positionOffsetRadius = 5f)
        {
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

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
                                    GetEnemies.Fan.prefabToSpawn,
                                    groundPosition,
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);
                                GameObject gameObject2 = UnityEngine.Object.Instantiate(
                                    GetEnemies.FlashTurret.prefabToSpawn,
                                    gameObject.transform.position,
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);

                                allPositions.Add(groundPosition);
                                var netobj = gameObject.GetComponent<NetworkObject>();
                                var netobj2 = gameObject2.GetComponent<NetworkObject>();
                                gameObject2.name = "Paparazzi";
                                netobj.Spawn(destroyWithScene: true);
                                netobj2.Spawn(destroyWithScene: true);
                                Networker.Instance.setSizeClientRPC(netobj.NetworkObjectId, new Vector3(0.2545f,1,1));
                                gameObject.transform.SetParent(gameObject2.transform);
                                spawnedMines++;

                            }
                        }
                    }
                    if (!validPositionFound)
                    {
                        MysteryDice.CustomLogger.LogWarning("Could not find a valid position for mine at spawn point: " + pos);
                    }
                }
                Networker.Instance.PlaySoundServerRPC("Paparazzi");
            }
        }public static float GetShortestDistanceSqr(Vector3 position, List<Vector3> positions)
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
