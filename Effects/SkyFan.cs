﻿using MysteryDice.Patches;
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
    internal class SkyFan : IEffect
    {
        public string Name => "Sky Fan";
        public static int MinMinesToSpawn = 1;
        public static int MaxMinesToSpawn = 1;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Suck or Blow, either way you're gonna go";
        public void Use()
        {
            Networker.Instance.SkyFanServerRPC();
        }

        public static void SpawnSkyFan(float positionOffsetRadius = 5f)
        {
            int MinesToSpawn = 1;
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
                                    GetEnemies.Fan.prefabToSpawn,
                                    groundPosition+new Vector3(0,250,0),
                                    Quaternion.identity,
                                    RoundManager.Instance.mapPropsContainer.transform);

                                allPositions.Add(groundPosition);
                                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                                var netobj = gameObject.GetComponent<NetworkObject>();
                                netobj.Spawn(destroyWithScene: true);
                                Networker.Instance.setSuctionServerRPC(netobj.NetworkObjectId);
                                spawnedMines++;
                                Networker.Instance.setSizeClientRPC(netobj.NetworkObjectId,new Vector3(45,75,45), Quaternion.Euler(90,0,0));
                                Networker.Instance.removeShadowsClientRPC(netobj.NetworkObjectId);

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

        public static void setSuck(GameObject fan)
        {
            CodeRebirth.src.Content.Maps.IndustrialFan fanScript = fan.GetComponent<CodeRebirth.src.Content.Maps.IndustrialFan>();
            fanScript.pushForce *= -.06f;
            fanScript.suctionForce *= .06f;
            fanScript.rotationSpeed *= .15f;
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
