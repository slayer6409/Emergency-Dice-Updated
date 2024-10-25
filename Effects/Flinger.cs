using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Flinger : IEffect
    {
        public string Name => "Flinger";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Get Flung";

        public void Use()
        {
            
            if (GetEnemies.Horse == null)
                return;
            GameObject enemyObject = UnityEngine.Object.Instantiate(
                           GetEnemies.Horse.enemyType.enemyPrefab,
                           StartOfRound.Instance.localPlayerController.transform.position,
                           Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            enemyObject.GetComponent<Surfaced.HorseAI>().launchForceIndex = 2;
            enemyObject.GetComponents<SkinnedMeshRenderer>()[0].materials[1].SetColor("red",Color.red);
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
        }
    }
}
