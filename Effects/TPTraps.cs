using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TPTraps : IEffect
    {
        public string Name => "TP Traps";
        public static int MinMinesToSpawn = 10;
        public static int MaxMinesToSpawn = 50;
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More Teleporter Traps spawned inside!";

        public void Use()
        {
            Networker.Instance.TPOverflowServerRPC();
        }

        public static void SpawnTeleporterTraps(int amount)
        {
            if (RoundManager.Instance == null || RoundManager.Instance.insideAINodes == null || GetEnemies.SpawnableTP == null || GetEnemies.SpawnableTP.prefabToSpawn == null || RoundManager.Instance.mapPropsContainer == null)
            {
                return;
            }

            List<Vector3> positions = new List<Vector3>();
            int spawnedMines = 0;
            List<GameObject> spawnPoints = RoundManager.Instance.insideAINodes.ToList();
            int totalSpawnPoints = spawnPoints.Count;
            while (spawnedMines < amount)
            {
                for (int i = 0; i < totalSpawnPoints && spawnedMines < amount; i++)
                {
                    Vector3 pos = spawnPoints[i].transform.position;

                    Vector3 position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(pos);

                    if (GetShortestDistanceSqr(position, positions) >= 1f)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(
                            GetEnemies.SpawnableTP.prefabToSpawn,
                            position,
                            Quaternion.identity,
                            RoundManager.Instance.mapPropsContainer.transform
                        );

                        positions.Add(position);
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                        gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                        spawnedMines++;
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
