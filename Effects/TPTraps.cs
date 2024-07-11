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

        public static int MaxMinesToSpawn = 30;
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More Teleporter Traps spawned inside!";

        public void Use()
        {
            Networker.Instance.TPOverflowServerRPC();
        }

        public static void SpawnTeleporterTraps(int amount)
        {
            MysteryDice.CustomLogger.LogError("Spawn TP Traps called with: "+ amount);

            if (RoundManager.Instance == null)
            {
                MysteryDice.CustomLogger.LogError("RoundManager.Instance is null.");
                return;
            }

            if (RoundManager.Instance.insideAINodes == null)
            {
                MysteryDice.CustomLogger.LogError("RoundManager.Instance.insideAINodes is null.");
                return;
            }

            if (GetEnemies.SpawnableTP == null)
            {
                MysteryDice.CustomLogger.LogError("GetEnemies.SpawnableTP is null.");
                return;
            }

            if (GetEnemies.SpawnableTP.prefabToSpawn == null)
            {
                MysteryDice.CustomLogger.LogError("GetEnemies.SpawnableTP.prefabToSpawn is null.");
                return;
            }

            if (RoundManager.Instance.mapPropsContainer == null)
            {
                MysteryDice.CustomLogger.LogError("RoundManager.Instance.mapPropsContainer is null.");
                return;
            }
            List<Vector3> positions = new List<Vector3>();
            int spawnedMines = 0;

            foreach (GameObject spawnpoint in RoundManager.Instance.insideAINodes)
            {
                Vector3 pos = spawnpoint.transform.position;

                if (spawnedMines > amount) return;

                Vector3 position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(pos);

                if (GetShortestDistanceSqr(position, positions) < 1f)
                    continue;
                
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
