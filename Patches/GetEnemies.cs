using BepInEx.Logging;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Security.AccessControl;
using System.Reflection;
using HarmonyLib.Tools;
using LethalLib.Extras;
using static LethalLib.Modules.ContentLoader;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public class GetEnemies
    {

        public static SpawnableEnemyWithRarity Masked, RedwoodTitan,  HoardingBug, Scary, Ghost, Boomba, Tulip, Centipede, Dog, Jester, Bracken, Stomper, Coilhead, Beehive, Sandworm, Spider, Giant, Maneater, Nutcracker, Shrimp, CrystalRay, Lasso, Barber, BellCrab, Urchin, Horse, Nemo, Bruce, MantisShrimp, Tornado;
        public static SpawnableMapObject SpawnableLandmine, SpawnableTurret, SpawnableTP, SpawnableSpikeTrap, Microwave, Fan, FlashTurret, Seamine, Bertha; 
        private static readonly string teleporterTrapId = "TeleporterTrap";
        public static List<EnemyType> allEnemies = new List<EnemyType>();
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void GetEnemy(Terminal __instance)
        {
            foreach (SelectableLevel level in __instance.moonsCatalogueList)
            {
                foreach (SpawnableEnemyWithRarity enemy in level.Enemies)
                {
                    if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogInfo("Enemy Found Inside: " + enemy.enemyType.enemyName);
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
                    if (enemy.enemyType.enemyName == "Maneater")
                        Maneater = enemy;
                    if (enemy.enemyType.enemyName == "Shrimp")
                        Shrimp = enemy;
                    if (enemy.enemyType.enemyName == "Crystal Ray")
                        CrystalRay = enemy;
                    if (enemy.enemyType.enemyName == "Lasso")
                        Lasso = enemy;
                    if (enemy.enemyType.enemyName == "Clay Surgeon")
                        Barber = enemy;
                    if (enemy.enemyType.enemyName == "Mantis Shrimp")
                        MantisShrimp = enemy;
                    if (enemy.enemyType.enemyName == "BellCrab")
                        BellCrab = enemy;
                    if (enemy.enemyType.enemyName == "Nutcracker")
                        Nutcracker = enemy;
                    if (enemy.enemyType.enemyName == "Girl")
                        Ghost = enemy;
                    if (enemy.enemyType.enemyName == "Boomba")
                        Boomba = enemy;
                    if (enemy.enemyType.enemyName == "Scary")
                        Scary = enemy;
                }

                foreach (SpawnableEnemyWithRarity enemy in level.DaytimeEnemies)
                {
                    if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogInfo("Enemy Found Daytime: " + enemy.enemyType.enemyName);
                    if (enemy.enemyType.enemyName == "Red Locust Bees")
                        Beehive = enemy;
                    if (enemy.enemyType.enemyName == "Urchin")
                        Urchin = enemy;
                    if (enemy.enemyType.enemyName == "Horse")
                        Horse = enemy;
                    if (enemy.enemyType.enemyName == "Nemo")
                        Nemo = enemy;
                    if (enemy.enemyType.enemyName == "Tulip Snake")
                        Tulip = enemy;
                }

                foreach (SpawnableEnemyWithRarity enemy in level.OutsideEnemies)
                {
                    if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogInfo("Enemy Found Outside: " + enemy.enemyType.enemyName);
                    if (enemy.enemyType.enemyName == "Earth Leviathan") 
                        Sandworm = enemy;
                    if (enemy.enemyType.enemyName == "ForestGiant")
                        Giant = enemy;
                    if (enemy.enemyType.enemyName == "MouthDog")
                        Dog = enemy;
                    if (enemy.enemyType.enemyName == "Bruce")
                        Bruce = enemy;
                    if (enemy.enemyType.enemyName == "Tornado")
                        Tornado = enemy;
                    if (enemy.enemyType.enemyName == "Redwood Titan")
                        RedwoodTitan = enemy;
                }

                foreach (var item in level.spawnableMapObjects)
                {
                    if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogInfo("Spawnable Map Object Found: " + item.prefabToSpawn.name);
                    if (item.prefabToSpawn.name == "Landmine" && SpawnableLandmine == null)
                        SpawnableLandmine = item;

                    if (item.prefabToSpawn.name == "TurretContainer" && SpawnableTurret == null)
                        SpawnableTurret = item;

                    if (item.prefabToSpawn.name == "SpikeRoofTrapHazard" && SpawnableSpikeTrap == null)
                        SpawnableSpikeTrap = item;

                    if (item.prefabToSpawn.name == "TeleporterTrap" && SpawnableTP == null)
                        SpawnableTP = item;

                    if (item.prefabToSpawn.name == "Seamine" && Seamine == null)
                        Seamine = item;

                    if (item.prefabToSpawn.name == "Bertha" && Bertha == null)
                        Bertha = item;

                    if (item.prefabToSpawn.name == "FunctionalMicrowave" && Microwave == null)
                        Microwave = item;

                    if (item.prefabToSpawn.name == "FanTrapAnimated" && Fan == null)
                        Fan = item;

                    if (item.prefabToSpawn.name == "FlashTurretUpdated" && FlashTurret == null)
                        FlashTurret = item;

                }
                
            }
        }
       
    }
}
