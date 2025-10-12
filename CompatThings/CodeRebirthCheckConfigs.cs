using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
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
    
    
    public static DawnMapObjectInfo getTrap(string trapName)
    {
        var trap = LethalContent.MapObjects.Where(x => x.Value.MapObject.name==trapName);
        var keyValuePairs = trap as KeyValuePair<NamespacedKey<DawnMapObjectInfo>, DawnMapObjectInfo>[] ?? trap.ToArray();
        if(keyValuePairs.Count()==0) return null;
        return keyValuePairs.First().Value;
        
    }
 
    public static EnemyType getEnemy(string enemyName)
    {
        var enemy = LethalContent.Enemies.Where(x => x.Value.EnemyType.enemyName==enemyName);
        var enemies = enemy as KeyValuePair<NamespacedKey<DawnEnemyInfo>, DawnEnemyInfo>[] ?? enemy.ToArray();
        if(enemies.Count()==0) return null;
        return enemies.First().Value.EnemyType;
    }

    public static void listAll()
    {
        foreach (var thing in LethalContent.MapObjects)
        {
            MysteryDice.ExtendedLogging(thing.Value.MapObject.name);   
        }
        foreach (var thing in LethalContent.Enemies)
        {
            MysteryDice.ExtendedLogging(thing.Value.EnemyType.enemyName);
        }
    }
    
    public static List<trap> getSpawnPrefabs()
    {
        
        List<trap> spawnPrefabs = new List<trap>();
        foreach (var trap in LethalContent.MapObjects)
        {
            MysteryDice.ExtendedLogging($"added {trap.Value.MapObject.name} to traps");
            spawnPrefabs.Add(new trap(trap.Value.MapObject.name, trap.Value.MapObject));
        }
        return spawnPrefabs;
    }

   
    
}