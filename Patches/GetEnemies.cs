using BepInEx.Logging;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class GetEnemies
    {

        public static SpawnableEnemyWithRarity Masked, HoardingBug, Centipede, Jester, Bracken, Stomper, Coilhead, Beehive, Sandworm, Spider;
        public static SpawnableMapObject SpawnableLandmine, SpawnableTurret;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void GetEnemy(Terminal __instance)
        {
            foreach (SelectableLevel level in __instance.moonsCatalogueList)
            {
                foreach (SpawnableEnemyWithRarity enemy in level.Enemies)
                {
                    if (enemy.enemyType.enemyName == "Masked")
                        Masked = enemy;
                    if (enemy.enemyType.enemyName == "Hoarding bug")
                        HoardingBug = enemy;
                    if (enemy.enemyType.enemyName == "Centipede")
                        Centipede = enemy;
                    if (enemy.enemyType.enemyName == "Jester")
                        Jester = enemy;
                    if (enemy.enemyType.enemyName == "Flowerman")
                        Bracken = enemy;
                    if (enemy.enemyType.enemyName == "Crawler")
                        Stomper = enemy;
                    if (enemy.enemyType.enemyName == "Spring")
                        Coilhead = enemy;
                    if (enemy.enemyType.enemyName == "Bunker Spider")
                        Spider = enemy;
                }

                foreach (SpawnableEnemyWithRarity enemy in level.DaytimeEnemies)
                {
                    if (enemy.enemyType.enemyName == "Red Locust Bees")
                        Beehive = enemy;
                }

                foreach (SpawnableEnemyWithRarity enemy in level.OutsideEnemies)
                {
                    if (enemy.enemyType.enemyName == "Earth Leviathan")
                    {
                        Sandworm = enemy;
                    }
                }

                foreach (var item in level.spawnableMapObjects)
                {
                    if (item.prefabToSpawn.name == "Landmine" && SpawnableLandmine == null)
                        SpawnableLandmine = item;

                    if (item.prefabToSpawn.name == "TurretContainer" && SpawnableTurret == null)
                        SpawnableTurret = item;
                }
            }
        }
    }
}
