using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MineOverflow : IEffect
    {
        public string Name => "Mine overflow";

        public static int MaxMinesToSpawn = 30;
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More mines spawned inside!";

        public void Use()
        {
            Networker.Instance.MineOverflowServerRPC();
        }

        public static void SpawnMoreMines(int amount)
        {
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
                    GetEnemies.SpawnableLandmine.prefabToSpawn,
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
            foreach(Vector3 pos in positions)
            {
                float distance = (position - pos).sqrMagnitude;
                if (distance < shortestLength)
                    shortestLength = distance;
            }
            return shortestLength;
        }
    }
}
