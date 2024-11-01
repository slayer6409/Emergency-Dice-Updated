using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class MineHardPlace : IEffect
    {
        public string Name => "Mine and a Hard Place";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can't go anywhere now";

        public void Use()
        {
            Networker.Instance.MineHardPlaceServerRPC(StartOfRound.Instance.localPlayerController.playerClientId);
        }
        public static void spawn(ulong playerID)
        {
            
            int BerthaSpawn = 8;
            if (GetEnemies.Bertha == null)
                return;
            var player = Misc.GetPlayerByUserID(playerID);
            float radius = 8;
            for (int i = 0; i < BerthaSpawn; i++)
            {
                float angle = i * Mathf.PI * 2 / BerthaSpawn;
                Vector3 spawnPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    player.transform.position.y + 0.25f,
                    Mathf.Sin(angle) * radius
                );
                spawnPosition += player.transform.position;
                var RM = RoundManager.Instance;
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Bertha.prefabToSpawn,
                    spawnPosition,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            }
        }
    }
}
