using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class CratesOutside : IEffect
    {
        public string Name => "Airsupply";
        public static int MinMinesToSpawn = 3;
        public static int MaxMinesToSpawn = 7;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Air support inbound";
        public void Use()
        {
            Networker.Instance.CratesOutsideServerRPC();
        }

        [MethodImpl (MethodImplOptions.NoInlining)]
        public static GameObject getRandomCrate()
        {
            var crates = CodeRebirth.src.Plugin.Mod.MapObjectRegistry()
                .Where(x => x.ObjectName.Contains("crate", StringComparison.OrdinalIgnoreCase)).ToList();
            return crates[UnityEngine.Random.Range(0, crates.Count)].GameObject;
        }

        [MethodImpl (MethodImplOptions.NoInlining)]
        public static void SpawnCratesOutside(int MinesToSpawn, float positionOffsetRadius = 5f)
        {
            
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

            int maxAttempts = 100;
            while (spawnedMines < MinesToSpawn)
            {
                for (int i = 0; spawnedMines < MinesToSpawn; i++)
                {

                    Vector3 position = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position;
                    Vector3 vector = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, default, random, -1) + (Vector3.up * 2);

                    Physics.Raycast(vector, Vector3.down, out RaycastHit hit, 100, StartOfRound.Instance.collidersAndRoomMaskAndDefault);
                    GameObject crate = getRandomCrate();

                    if (hit.collider != null)
                    {
                        Vector3 spawnPoint = hit.point + hit.normal * -0.75f;
                        GameObject spawnedCrate = GameObject.Instantiate(crate, spawnPoint, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
                        spawnedCrate.transform.up = hit.normal;
                        SceneManager.MoveGameObjectToScene(spawnedCrate, RoundManager.Instance.mapPropsContainer.scene);
                        spawnedCrate.GetComponent<NetworkObject>().Spawn();
                        spawnedMines++;
                    }
                }
            }
        }
        
        [MethodImpl (MethodImplOptions.NoInlining)]
        public static void SpawnCratesInside(int MinesToSpawn, float positionOffsetRadius = 5f)
        {
           
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

            while (spawnedMines < MinesToSpawn)
            {
                for (int i = 0; spawnedMines < MinesToSpawn; i++)
                {

                    Vector3 position = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length - 1)].transform.position;
                    Vector3 vector = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, default, random, -1) + (Vector3.up * 2);

                    Physics.Raycast(vector, Vector3.down, out RaycastHit hit, 100, StartOfRound.Instance.collidersAndRoomMaskAndDefault);
                    
                    GameObject crate = getRandomCrate();
                    

                    if (hit.collider != null)
                    {
                        Vector3 spawnPoint = hit.point + hit.normal * -0.75f;
                        GameObject spawnedCrate = GameObject.Instantiate(crate, spawnPoint, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
                        spawnedCrate.transform.up = hit.normal;
                        SceneManager.MoveGameObjectToScene(spawnedCrate, RoundManager.Instance.mapPropsContainer.scene);
                        spawnedCrate.GetComponent<NetworkObject>().Spawn();
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
    internal class CratesInside : IEffect
    {
        public string Name => "Amazon Shipping";
        public static int MinMinesToSpawn = 2;
        public static int MaxMinesToSpawn = 5;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Amazon left your boxes inside";
        public void Use()
        {
            Networker.Instance.CratesInsideServerRPC();
        }
    }
}
