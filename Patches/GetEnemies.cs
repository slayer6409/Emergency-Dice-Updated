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

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class GetEnemies
    {

        public static SpawnableEnemyWithRarity Masked, HoardingBug, Centipede, Jester, Bracken, Stomper, Coilhead, Beehive, Sandworm, Spider;
        public static SpawnableMapObject SpawnableLandmine, SpawnableTurret, SpawnableTP; 
        private static readonly string teleporterTrapId = "TeleporterTrap"; 

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
                if (MysteryDice.lethalThingsPresent)
                {
                    var lethalThingsAssembly = MysteryDice.lethalThingsAssembly;
                    if (lethalThingsAssembly != null)
                    {
                        MysteryDice.CustomLogger.LogWarning("Checking.");
                        CheckForTeleporterTrap();

                    }
                }
            }
        }
        private static void CheckForTeleporterTrap()
        {
            MysteryDice.CustomLogger.LogWarning("Checking for TeleporterTrap...");

            var lethalThingsAssembly = MysteryDice.lethalThingsAssembly;
            if (lethalThingsAssembly != null)
            {
                Type contentType = lethalThingsAssembly.GetType("LethalThings.Content");
                if (contentType != null)
                {
                    FieldInfo contentLoaderField = contentType.GetField("ContentLoader", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (contentLoaderField != null)
                    {
                        var contentLoaderInstance = contentLoaderField.GetValue(null);
                        if (contentLoaderInstance != null)
                        {
                            PropertyInfo loadedContentProperty = contentLoaderInstance.GetType().GetProperty("LoadedContent", BindingFlags.Instance | BindingFlags.Public);
                            if (loadedContentProperty != null)
                            {
                                var loadedContent = loadedContentProperty.GetValue(contentLoaderInstance) as IDictionary;
                                if (loadedContent != null && loadedContent.Contains(teleporterTrapId))
                                {
                                    var teleporterTrap = loadedContent[teleporterTrapId] as MapHazard;
                                    if (teleporterTrap != null)
                                    {
                                        FieldInfo hazardField = typeof(MapHazard).GetField("hazard", BindingFlags.Instance | BindingFlags.NonPublic);
                                        if (hazardField != null)
                                        {
                                            var spawnableMapObjectDef = hazardField.GetValue(teleporterTrap) as SpawnableMapObjectDef;
                                            if (spawnableMapObjectDef != null)
                                            {
                                                SpawnableTP = spawnableMapObjectDef.spawnableMapObject;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        private static void ListAllTypesInAssembly(string AssemblyName)
        {
            MysteryDice.CustomLogger.LogWarning($"Starting to list all types in {AssemblyName} assembly...");

            var theAssembly = MysteryDice.GetAssembly(AssemblyName);
            if (theAssembly != null)
            {
                MysteryDice.CustomLogger.LogWarning($"{AssemblyName} assembly found.");

                foreach (Type type in theAssembly.GetTypes())
                {
                    MysteryDice.CustomLogger.LogWarning($"Type found: {type.FullName}");
                }
            }
            else
            {
                MysteryDice.CustomLogger.LogWarning($"{AssemblyName} assembly not found.");
            }
        }
        
    }
}
