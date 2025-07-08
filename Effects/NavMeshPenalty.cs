using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class NavMeshPenalty : IEffect
    {
        public string Name => "NavMeshPenalty";
        public EffectType Outcome => EffectType.Penalty;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You shouldn't have done that";


        public void Use()
        {
            Networker.Instance.doPenaltyServerRPC(10);
        }

        public static void doPenalty(int amount)
        {
            List<SpawnableEnemyWithRarity> allenemies = StartOfRound.Instance.currentLevel.Enemies
                .Union(StartOfRound.Instance.currentLevel.OutsideEnemies)
                .Union(StartOfRound.Instance.currentLevel.DaytimeEnemies)
                .ToList();

            List<SpawnableEnemyWithRarity> randomEnemies = new List<SpawnableEnemyWithRarity>();
            for (int i = 0; i < amount; i++)
            {
                var randomEnemy = allenemies[UnityEngine.Random.Range(0, allenemies.Count)];
                randomEnemies.Add(randomEnemy);
            }

            foreach (var enemy in randomEnemies)
            {
                Misc.SpawnEnemyForced(enemy, 1, false);
            }
        }
    }
}
