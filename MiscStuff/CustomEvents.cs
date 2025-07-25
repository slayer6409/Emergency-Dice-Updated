﻿using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using MysteryDice.Dice;
using MysteryDice.Effects;
using MysteryDice.Patches;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.MiscStuff
{
    public class CustomEnemyConfig
    {
        public string monsterName {  get; set; }
        public EffectType outcome { get; set; }
        public bool IsInside { get; set; }
        public int AmountMin { get; set; }
        public int AmountMax { get; set; }
        public string customName { get; set; }
        public string customTooltip { get; set; }
    }
    public class CustomItemConfig
    {
        public string itemName {  get; set; }
        public EffectType outcome { get; set; }
        public int AmountMax { get; set; }
        public string customName { get; set; }
        public string customTooltip { get; set; }
    }
    public class CustomTrapConfig
    {
        public string trapName {  get; set; }
        public EffectType outcome { get; set; }
        public bool IsInside { get; set; }
        public int AmountMax { get; set; }
        
        public bool moving { get; set; }
        public string customName { get; set; }
        public string customTooltip { get; set; }
    }
    public class CustomConfigs
    {
        private ConfigFile configFile;
        public List<CustomEnemyConfig> EnemyConfigs { get; private set; } = new List<CustomEnemyConfig>();
        public List<CustomItemConfig> ItemConfigs { get; private set; } = new List<CustomItemConfig>();
        public List<CustomTrapConfig> TrapConfigs { get; private set; } = new List<CustomTrapConfig>();
       
        public CustomConfigs(ConfigFile config)
        {
            configFile = config;
        }

        public void GenerateConfigs(int numberOfEnemyConfigs, int numberOfItemConfigs, int numberOfTrapConfigs)
        {
            for (int i = 1; i <= numberOfEnemyConfigs; i++)
            {
                // Create new config options
                string monsterName = configFile.Bind<string>(
                    $"CustomEnemy{i}",
                    "Monster Name",
                    "Locker",
                    $"The name of Custom Enemy{i}\nMake sure to get the EXACT name of the enemy\nYou can use the DebugLogging config and go to a moon to see all enemy names").Value;

                string customName = configFile.Bind<string>(
                    $"CustomEnemy{i}",
                    "Enemy Display Name",
                    "School Season",
                    $"Sets the Display name \"Player Rolled Item Display Name\"").Value;

                string customTooltip = configFile.Bind<string>(
                    $"CustomEnemy{i}",
                    "Enemy Tooltip Name",
                    "Don't scan them!",
                    $"This is the Tooltip that shows on the popup for the dice").Value;

                bool isInside = configFile.Bind<bool>(
                    $"CustomEnemy{i}",
                    "Is Inside",
                    false,
                    $"Does the enemy Spawn inside for Custom{i}").Value;

                EffectType outcome = configFile.Bind<EffectType>(
                    $"CustomEnemy{i}",
                    "Outcome",
                    EffectType.Bad,
                    $"Sets the Outcome for Custom{i}").Value;

                int amountMin = configFile.Bind<int>(
                    $"CustomEnemy{i}",
                    "Amount Min",
                    1, // Default value
                    $"Minimum number of enemies to spawn for Custom{i}").Value;
                int amountMax = configFile.Bind<int>(
                    $"CustomEnemy{i}",
                    "Amount Max",
                    5, // Default value
                    $"Maximum number of enemies to spawn for Custom{i}").Value;


                // Create a new custom enemy config
                CustomEnemyConfig enemyConfig = new CustomEnemyConfig
                {
                    monsterName = monsterName,
                    outcome = outcome,
                    IsInside = isInside,
                    AmountMin = amountMin,
                    AmountMax = amountMax,
                    customName = customName,
                    customTooltip = customTooltip
                    
                };
                EnemyConfigs.Add(enemyConfig);

                // Register the effect using the dynamic effect class
                DieBehaviour.AllEffects.Add(new DynamicEffect($"{customName}", enemyConfig));
            }
            for (int i = 1; i <= numberOfItemConfigs; i++)
            {
                // Create new config options
                string itemName = configFile.Bind<string>(
                    $"CustomItem{i}",
                    "Item Name",
                    "Key",
                    $"The name of Item{i}\nMake sure to get the EXACT name of the Item\nYou can use the DebugLogging config and pick up the item to see it's name").Value;
                
                string customName = configFile.Bind<string>(
                    $"CustomItem{i}",
                    "Item Display Name",
                    "Door Opener",
                    $"Sets the Display name \"Player Rolled Item Display Name\"").Value;
                
                string customTooltip = configFile.Bind<string>(
                    $"CustomItem{i}",
                    "Item Tooltip Name",
                    "They Open Doors!",
                    $"This is the Tooltip that shows on the popup for the dice").Value;

                EffectType outcome = configFile.Bind<EffectType>(
                    $"CustomItem{i}",
                    "Outcome",
                    EffectType.Good,
                    $"Sets the Outcome for Custom{i}").Value;

                int amountMax = configFile.Bind<int>(
                    $"CustomItem{i}",
                    "Amount Max",
                    5, // Default value
                    $"Maximum number of Items to spawn for Custom{i}").Value;

                // Create a new custom enemy config
                CustomItemConfig itemConfig = new CustomItemConfig
                {
                    itemName = itemName,
                    outcome = outcome,
                    AmountMax = amountMax,
                    customName = customName,
                    customTooltip = customTooltip

                };
                ItemConfigs.Add(itemConfig);

                // Register the effect using the dynamic effect class
                DieBehaviour.AllEffects.Add(new DynamicItemEffect($"{customName}", itemConfig));
            }
            for (int i = 1; i <= numberOfTrapConfigs; i++)
            {
                // Create new config options
                string trapName = configFile.Bind<string>(
                    $"CustomTrap{i}",
                    "Trap Name",
                    "CageMine",
                    $"The name of Trap{i}\nMake sure to get the EXACT name of the Trap\nYou can use the DebugLogging config and go to a moon to see all SpawnableMapObjects names").Value;
                
                string customName = configFile.Bind<string>(
                    $"CustomTrap{i}",
                    "Trap Display Name",
                    "Cage Mines?",
                    $"Sets the Display name \"Player Rolled Trap Display Name\"").Value;
                
                string customTooltip = configFile.Bind<string>(
                    $"CustomTrap{i}",
                    "Trap Tooltip Name",
                    "It's Gonna Trap Ya",
                    $"This is the Tooltip that shows on the popup for the dice").Value;

                bool isInside = configFile.Bind<bool>(
                    $"CustomTrap{i}",
                    "Is Inside",
                    false,
                    $"Does the Trap Spawn inside for Custom{i}").Value;

                EffectType outcome = configFile.Bind<EffectType>(
                    $"CustomTrap{i}",
                    "Outcome",
                    EffectType.Awful,
                    $"Sets the Outcome for Custom{i}").Value;

                int amountMax = configFile.Bind<int>(
                    $"CustomTrap{i}",
                    "Amount Max",
                    5, // Default value
                    $"Maximum number of Traps to spawn for Custom{i}").Value;
                
                bool ismoving = configFile.Bind<bool>(
                    $"CustomTrap{i}",
                    "Moving",
                    false, // Default value
                    $"Do traps of the type: {i} move").Value;

                // Create a new custom enemy config
                CustomTrapConfig trapConfig = new CustomTrapConfig
                {
                    trapName = trapName,
                    outcome = outcome,
                    IsInside = isInside,
                    moving = ismoving,
                    AmountMax = amountMax,
                    customName = customName,
                    customTooltip = customTooltip

                };
                TrapConfigs.Add(trapConfig);

                // Register the effect using the dynamic effect class
                MysteryDice.MainRegisterNewEffect(new DynamicTrapEffect($"{customName}", trapConfig),false,false);
            }
        }
    }

    internal class DynamicEffect : IEffect
    {
        private string name;
        private CustomEnemyConfig config;

        public DynamicEffect(string name, CustomEnemyConfig config)
        {
            this.name = name;
            this.config = config;
        }

        public string Name => name;
        public EffectType Outcome => config.outcome; // Adjust as necessary
        public bool ShowDefaultTooltip => true;
        public string Tooltip => config.customTooltip;
        public static EnemyType enemy = null;
        public void Use()
        {
            Networker.Instance.CustomMonsterServerRPC(config.monsterName, config.AmountMin, config.AmountMax, config.IsInside);
        }
        public static void spawnEnemy(string names, int min, int max, bool inside, Vector3 size = default)
        {
            var enemyNames = names.Split(',');
            foreach (var name in enemyNames)
            {
                int randomSpawn = 0; 
                // List<SpawnableEnemyWithRarity> allenemies = new List<SpawnableEnemyWithRarity>();
                //
                // foreach (var level in StartOfRound.Instance.levels)
                // {
                //     allenemies = allenemies
                //         .Union(level.Enemies)
                //         .Union(level.OutsideEnemies)
                //         .Union(level.DaytimeEnemies)
                //         .ToList();
                // }
                // allenemies = allenemies
                //     .GroupBy(x => x.enemyType.enemyName)
                //     .Select(g => g.First())
                //     .OrderBy(x => x.enemyType.enemyName)
                //     .ToList();
                
                var allEnemies = GetEnemies.allEnemies.OrderBy(x => x.enemyName).ToList();
                enemy = allEnemies.FirstOrDefault(x => x.enemyName == name);
                // if (enemy == null)
                // { //do original method as backup
                //     foreach (SelectableLevel level in StartOfRound.Instance.levels)
                //     {
                //
                //         enemy = level.Enemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                //         if (enemy == null)
                //             enemy = level.DaytimeEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                //         if (enemy == null)
                //             enemy = level.OutsideEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                //
                //
                //     }
                // }
                if (enemy == null)
                {
                    MysteryDice.CustomLogger.LogWarning($"Enemy '{name}' not found. Available enemies: {string.Join(", ", allEnemies.Select(e => e.enemyName))}"); return;
                }
                randomSpawn = UnityEngine.Random.Range(min, max + 1);
                Misc.SpawnEnemy(enemy, randomSpawn, inside);
            }
           
        }
    }
    internal class DynamicItemEffect : IEffect
    {
        private string name;
        private CustomItemConfig config;

        public DynamicItemEffect(string name, CustomItemConfig config)
        {
            this.name = name;
            this.config = config;
        }

        public string Name => name;
        public EffectType Outcome => config.outcome; // Adjust as necessary
        public bool ShowDefaultTooltip => true;
        public string Tooltip => config.customTooltip;
        public void Use()
        {
            Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), UnityEngine.Random.Range(1, config.AmountMax+1), config.itemName);
        }
    }
    internal class DynamicTrapEffect : IEffect
    {
        private string name;
        private CustomTrapConfig config;

        public DynamicTrapEffect(string name, CustomTrapConfig config)
        {
            this.name = name;
            this.config = config;
        }

        public string Name => name;
        public EffectType Outcome => config.outcome; // Adjust as necessary
        public bool ShowDefaultTooltip => true;
        public string Tooltip => config.customTooltip;
        public void Use()
        {
            Networker.Instance.CustomTrapServerRPC(config.AmountMax,config.trapName, config.IsInside, config.moving);
        }
        public static trap getTrap(string name)
        {
            var allTraps = Misc.getAllTraps();
            var trap = allTraps.FirstOrDefault(x => x.name == name);
            if (trap == null) trap = allTraps.FirstOrDefault(x => x.name == "Landmine");
            return trap;
            
        }
        public static void spawnTrap(int max, string trapNames, bool inside, float positionOffsetRadius = 5f, bool moving = false, bool useScale = false, Vector3 scale = default)
        {
            var traps = trapNames.Split(',');
            foreach (var trapName in traps)
            {
                List<Vector3> allPositions = new List<Vector3>();
                int spawnedMines = 0;
                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

                // Get all spawn points
                List<GameObject> spawnPoints;
                if (!inside) 
                {
                    spawnPoints = RoundManager.Instance.outsideAINodes.ToList();
                }
                else
                {
                    spawnPoints = RoundManager.Instance.insideAINodes.ToList();
                }
                int totalSpawnPoints = spawnPoints.Count;

                if (totalSpawnPoints == 0)
                {
                    return;
                }
                int maxAttempts = 100;

                var trap = getTrap(trapName);
                int MinesToSpawn = UnityEngine.Random.Range(3, max + 1);
                while (spawnedMines < MinesToSpawn)
                {
                    for (int i = 0; i < totalSpawnPoints && spawnedMines < MinesToSpawn; i++)
                    {
                        Vector3 pos = spawnPoints[Random.Range(0,totalSpawnPoints)].transform.position;
                        bool validPositionFound = false;
                        for (int attempt = 0; attempt < maxAttempts && !validPositionFound; attempt++)
                        {
                            Vector3 offset = new Vector3(
                                (float)(random.NextDouble() * 2 - 1) * positionOffsetRadius,
                                0,
                                (float)(random.NextDouble() * 2 - 1) * positionOffsetRadius);

                            Vector3 randomPosition = pos + offset;
                            if (Physics.Raycast(randomPosition + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f))
                            {
                                Vector3 groundPosition = hit.point;
                                if (GetShortestDistanceSqr(groundPosition, allPositions) >= 1)
                                {
                                    validPositionFound = true;

                                    GameObject gameObject = UnityEngine.Object.Instantiate(
                                        trap.prefab,
                                        groundPosition,
                                        Quaternion.identity,
                                        RoundManager.Instance.mapPropsContainer.transform);

                                    allPositions.Add(groundPosition);
                                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                                    var netobj = gameObject.GetComponent<NetworkObject>();
                                    netobj.Spawn(destroyWithScene: true);
                                    spawnedMines++;
                                    if(useScale) Networker.Instance.setSizeClientRPC(netobj.NetworkObjectId, scale, Quaternion.identity);
                                }
                            }
                        }

                        if (!validPositionFound)
                        {
                            Debug.LogWarning("Could not find a valid position for mine at spawn point: " + pos);
                        }
                    }
                }

                if (spawnedMines > 0 && moving)
                {
                    Networker.Instance.AddMovingTrapClientRPC(trap.name);
                }
                
            }
        }
        public static float GetShortestDistanceSqr(Vector3 position, List<Vector3> positions)
        {
            float shortestLength = float.MaxValue;
            foreach (Vector3 pos in positions)
            {
                float distance = (position - pos).sqrMagnitude;
                if (distance < shortestLength)
                    shortestLength = distance;
            }
            return shortestLength;
        }
    }
}
