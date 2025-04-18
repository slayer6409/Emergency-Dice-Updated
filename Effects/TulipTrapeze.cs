using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Object = UnityEngine.Object;

namespace MysteryDice.Effects
{
    internal class TulipTrapeze : IEffect
    {
        public string Name => "Tulip Trapeze";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Hope you like flying";

        public void Use()
        {
            Networker.Instance.TulipTrapeezeServerRPC();
        }
        public static void spawn()
        {
            bool removeAfter = false;
            var RM = RoundManager.Instance;
            int TulipSpawn = UnityEngine.Random.Range(10, 16);
            if (GetEnemies.Tulip == null)
                return;
            var player = Misc.GetRandomAlivePlayer();
            Networker.Instance.TulipTrapeezeMessageServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
            player.DropAllHeldItemsAndSync();
            float radius = 2;
            if (!RoundManager.Instance.currentLevel.Enemies.Contains(GetEnemies.Tulip))
            {
                RoundManager.Instance.currentLevel.Enemies.Add(GetEnemies.Tulip);
                removeAfter = true;
            }

            var entrance = Object.FindObjectsOfType<EntranceTeleport>()
                .FirstOrDefault((EntranceTeleport e) => e.entranceId == 0 && e.isEntranceToBuilding);
            Vector3 pos = entrance.entrancePoint.position;
            if(entrance) player.TeleportPlayer(pos,false);
            for (int i = 0; i < TulipSpawn; i++)
            {
                float angle = i * Mathf.PI * 2 / TulipSpawn;
                Vector3 spawnPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    player.transform.position.y + 0.25f,
                    Mathf.Sin(angle) * radius
                );
                spawnPosition += player.transform.position;
                Vector3 directionToPlayer = player.transform.position - spawnPosition;
                Quaternion rotation = Quaternion.LookRotation(directionToPlayer);
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Tulip.enemyType.enemyPrefab,
                    spawnPosition,
                    rotation);
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            }
            if (removeAfter)
            {
                RoundManager.Instance.currentLevel.Enemies.Remove(GetEnemies.Tulip);
                removeAfter = false;
            }
        }
    }
}
