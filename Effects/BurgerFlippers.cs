using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class BurgerFlippers : IEffect
    {
        public string Name => "Burger Flippers";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "WEEEEEEEEEEE";

        public void Use()
        {
            var RM = RoundManager.Instance;
            int HorseSpawn = UnityEngine.Random.Range(6, 10);
            if (GetEnemies.Horse == null)
                return;
            var player = StartOfRound.Instance.localPlayerController;
            float radius = 3;
            for (int i = 0; i < HorseSpawn; i++) 
            {
                float angle = i * Mathf.PI * 2 / HorseSpawn;
                Vector3 spawnPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    player.transform.position.y + 0.25f,
                    Mathf.Sin(angle) * radius
                ); 
                Vector3 directionToPlayer = player.transform.position - spawnPosition;
                Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

                spawnPosition += player.transform.position;
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Horse.enemyType.enemyPrefab,
                    spawnPosition,
                    rotation);
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            }
        }
    }
}
