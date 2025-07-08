using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.MapObjects;
using UnityEngine;

namespace MysteryDice.CompatThings;

public class CodeRebirthCheckConfigs
{
    /*
        Trap: Oxyde Crashed Ship
        Trap: Compactor Toby
        Trap: Gunslinger Greg
        Trap: Shredder Sarah
        Trap: Merchant
        Trap: Money
        Trap: Normal Metal Crate
        Trap: Mimic Metal Crate
        Trap: Mimic Wooden Crate
        Trap: Normal Wooden Crate
        Trap: Air Control Unit
        Trap: Flash Turret
        Trap: Boom Trap
        Trap: Grass Bear Trap
        Trap: Gravel Bear Trap
        Trap: Snow Bear Trap
        Trap: Functional Microwave
        Trap: Autonomous Crane
        Trap: Laser Turret
        Trap: Industrial Fan
        Trap: Emerging Cactus 1
        Trap: Emerging Cactus 2
        Trap: Emerging Cactus 3
        Trap: Trash Can
        
        Enemy: Tornado
        Enemy: Guardsman
        Enemy: Cactus Budling
        Enemy: Rabbit Magician
        Enemy: Peace Keeper
        Enemy: Lord Of The Manor
        Enemy: Janitor
        Enemy: Driftwood Menace
        Enemy: Nancy
        Enemy: CutieFly
        Enemy: Monarch
        Enemy: Mistress
        Enemy: Redwood Titan
        Enemy: Carnivorous Plant
        Enemy: SnailCat
        Enemy: Duck
        Enemy: Transporter
     */
    
    
    public static GameObject getTrap(string trapName)
    {
        CodeRebirth.src.Plugin.Mod.MapObjectRegistry().TryGetFromMapObjectName(trapName, out var trap);
        return trap.GameObject;
    }
 
    public static EnemyType getEnemy(string enemyName)
    {
        CodeRebirth.src.Plugin.Mod.EnemyRegistry().TryGetFromEnemyName(enemyName, out var enemy);
        return enemy?.EnemyType;
    }
    
    public static void ListAll()
    {
        foreach (CRMapObjectDefinition def in CodeRebirth.src.Plugin.Mod.MapObjectRegistry())
        {
            MysteryDice.ExtendedLogging("Trap: "+def.ObjectName);
        }
        foreach (var def in CodeRebirth.src.Plugin.Mod.EnemyRegistry())
        {
            MysteryDice.ExtendedLogging("Enemy: "+def.EnemyType.enemyName);
        }
    }
    public static List<trap> getSpawnPrefabs()
    {
        List<trap> spawnPrefabs = new List<trap>();
        foreach (var trap in CRMod.AllMapObjects())
        {
            MysteryDice.ExtendedLogging($"added {trap.ObjectName} to traps");
            spawnPrefabs.Add(new trap(trap.ObjectName, trap.GameObject));
        }
        return spawnPrefabs;
    }

   
    
}