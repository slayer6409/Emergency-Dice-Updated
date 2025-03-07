using BepInEx.Configuration;
using MysteryDice.Patches;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class CleaningCrew : IEffect
    {
        public string Name => "Cleaning Crew";

        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "They are doing a good job";

        public void Use()
        {
            var cross = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(x => x.playerSteamId == 76561199092131418);
            if (cross != null)
            {
                if (!cross.isPlayerDead) Networker.Instance.CleaningCrewServerRPC(cross.actualClientId);
                else Networker.Instance.CleaningCrewServerRPC(Misc.GetRandomAlivePlayer().actualClientId);
            }
            else
            {
                Networker.Instance.CleaningCrewServerRPC(Misc.GetRandomAlivePlayer().actualClientId);
            }
        }

        public static void spawnCrew(ulong playerId)
        {
            var player = Misc.GetPlayerByUserID(playerId);
            var toSpawn = Random.Range(3, 6);
            for (int i = 0; i < toSpawn; i++)
            {
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Jimothy.enemyType.enemyPrefab,
                    player.transform.position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
                GameObject enemyObject2 = UnityEngine.Object.Instantiate(
                    GetEnemies.Janitor.enemyType.enemyPrefab,
                    player.transform.position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject2.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RoundManager.Instance.SpawnedEnemies.Add(enemyObject2.GetComponent<EnemyAI>());
            }
            
        }

       
    }
}
