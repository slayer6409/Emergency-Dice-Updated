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
    internal class MineOverflowOutside : IEffect
    {
        public string Name => "Mine overflow outside";
        public static int MaxMinesToSpawn = 80;
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Mines spawned outside!";
        public void Use()
        {
            Networker.Instance.MineOverflowOutsideServerRPC();
        }

        public static void SpawnMoreMinesOutside()
        {
            List<Vector3> positions = new List<Vector3>();
            int spawnedMines = 0;
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
            
            foreach(GameObject spawnpoint in RoundManager.Instance.outsideAINodes)
            {
                Vector3 pos = spawnpoint.transform.position;
                for (int i = 0; i < 8; i++)
                {
                    if (spawnedMines > MaxMinesToSpawn) return;

                    Vector3 position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(
                        pos,
                        15f,
                        RoundManager.Instance.navHit,
                        random);

                    if (MineOverflow.GetShortestDistanceSqr(position, positions) < 1)
                        continue;

                    GameObject gameObject = UnityEngine.Object.Instantiate(
                    GetEnemies.SpawnableLandmine.prefabToSpawn,
                    position,
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);

                    positions.Add(position);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    spawnedMines++;
                }
            }
        }
    }
}
