using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class OutsideCoilhead : IEffect
    {
        public string Name => "Outside coilhead";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spawned a coilhead outside!";

        public void Use()
        {
            Networker.Instance.OutsideCoilheadServerRPC();
        }

        public static void SpawnOutsideCoilhead()
        {
            if (GetEnemies.Coilhead == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.Coilhead, 1, true);

            GameObject gameObject = GameObject.Instantiate(GetEnemies.Coilhead.enemyType.enemyPrefab, GetMainEntrancePosition(),Quaternion.identity);
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());

            Misc.SetNavmesh(gameObject.GetComponent<EnemyAI>(), true);

            NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();
            Networker.Instance.StartCoroutine(Networker.Instance.ServerDelayedCoilheadSetProperties(netObj));
        }

        public static Vector3 GetMainEntrancePosition()
        {
            EntranceTeleport[] allEntrances = GameObject.FindObjectsOfType<EntranceTeleport>();
            EntranceTeleport mainEntrance = null;
            for (int i = 0; i < allEntrances.Length; i++)
            {
                if (allEntrances[i].isEntranceToBuilding)
                {
                    mainEntrance = allEntrances[i];
                    break;
                }
            }

            if (mainEntrance == null) return Vector3.zero;

            return mainEntrance.transform.position;
        }

        
    }
}
