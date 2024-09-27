using BepInEx.Configuration;
using MysteryDice.Dice;
using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InvisibleEnemy : IEffect
    {
        public string Name => "Invisible Enemy";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Make sure you keep scanning for enemies!";
        public static ConfigEntry<bool> spider;
        public static ConfigEntry<bool> HoardingBug;
        public static ConfigEntry<bool> CoilHead;
        public static ConfigEntry<bool> Jester;
        public static ConfigEntry<bool> Centipede;
        public static ConfigEntry<bool> Bracken;
        public static ConfigEntry<bool> Barber;
        public static List<SpawnableEnemyWithRarity> spawnableEnemies = new List<SpawnableEnemyWithRarity>(); 
        public void Use()
        {
            spawnableEnemies.Clear();
            if(spider.Value && GetEnemies.Spider != null) spawnableEnemies.Add(GetEnemies.Spider);
            if(HoardingBug.Value && GetEnemies.HoardingBug!=null) spawnableEnemies.Add(GetEnemies.HoardingBug); 
            if(CoilHead.Value && GetEnemies.Coilhead != null) spawnableEnemies.Add(GetEnemies.Coilhead); 
            if(Jester.Value && GetEnemies.Jester != null) spawnableEnemies.Add(GetEnemies.Jester); 
            if(Centipede.Value && GetEnemies.Centipede != null) spawnableEnemies.Add(GetEnemies.Centipede);
            if(Bracken.Value && GetEnemies.Bracken != null) spawnableEnemies.Add(GetEnemies.Bracken);
            if(Barber.Value && GetEnemies.Barber != null) spawnableEnemies.Add(GetEnemies.Barber);

            var e = UnityEngine.Random.Range(0, spawnableEnemies.Count);
            Misc.SpawnEnemyForced(spawnableEnemies[e], 1, true, true);
        }

        public static void Config()
        {
            spider = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Spider",
               true,
               "Lets the invisible enemy event spawn an invisible spider");

            HoardingBug = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Hoarding Bug",
               true,
               "Lets the invisible enemy event spawn an invisible Hoarding Bug");

            CoilHead = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn CoilHead",
               true,
               "Lets the invisible enemy event spawn an invisible CoilHead");

            Jester = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Jester",
               true,
               "Lets the invisible enemy event spawn an invisible Jester");

            Centipede = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Centipede",
               true,
               "Lets the invisible enemy event spawn an invisible Centipede");

            Bracken = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Bracken",
               true,
               "Lets the invisible enemy event spawn an invisible Bracken");

            Barber = MysteryDice.BepInExConfig.Bind<bool>(
               "InvisibleEnemy",
               "Spawn Barber",
               true,
               "Lets the invisible enemy event spawn an invisible Barber");
        }
    }
}
