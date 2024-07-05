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
    internal class TurretHell : IEffect
    {
        public string Name => "Turret hell";
        public static int MaxTurretsToSpawn = 10;
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More turrets spawned inside!";

        public void Use()
        {
            Networker.Instance.TuretHellServerRPC();
        }

        public static void SpawnTurrets(int amount)
        {
            int spawnedTurrets = 0;

            foreach (GameObject spawnpoint in RoundManager.Instance.insideAINodes)
            {
                Vector3 pos = spawnpoint.transform.position;

                if (spawnedTurrets > amount) return;

                pos = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(pos);

                GameObject gObj = UnityEngine.Object.Instantiate(
                    GetEnemies.SpawnableTurret.prefabToSpawn,
                    pos,
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform
                );

                gObj.transform.eulerAngles = new Vector3(gObj.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gObj.transform.eulerAngles.z);
                gObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                spawnedTurrets++;             
            }
        }

    }
}
