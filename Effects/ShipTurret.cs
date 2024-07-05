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
    internal class ShipTurret : IEffect
    {
        public string Name => "Ship Turret";

        public static int MaxTurretsToSpawn = 4;
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Turret spawned near the ship!";

        public void Use()
        {
            Networker.Instance.ShipTurretServerRPC();
        }

        public static void SpawnTurretsShip(int amount)
        {
            Transform catwalkShip = GameObject.Find("CatwalkShip").transform;
            for (int i = 0; i < amount; i++)
            {
                Vector3 pos = catwalkShip.position;

                pos = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(pos,5f);

                GameObject gObj = UnityEngine.Object.Instantiate(
                    GetEnemies.SpawnableTurret.prefabToSpawn,
                    pos,
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform
                );

                gObj.transform.eulerAngles = new Vector3(gObj.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gObj.transform.eulerAngles.z);
                gObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
            }
        }

    }
}
