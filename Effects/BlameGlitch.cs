using BepInEx.Configuration;
using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static MysteryDice.Effects.SizeDifference;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class BlameGlitch : IEffect
    {
        public string Name => "Blame Glitch";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "BlameGlitch.exe";


        public static ConfigEntry<int> minNum;
        public static ConfigEntry<int> maxNum;
        public static ConfigEntry<bool> isInside;
        public static ConfigEntry<bool> bothInsideOutside;


        public void Use()
        {
            List<SpawnableEnemyWithRarity> allenemies = StartOfRound.Instance.currentLevel.Enemies
            .Union(StartOfRound.Instance.currentLevel.OutsideEnemies)
            .Union(StartOfRound.Instance.currentLevel.DaytimeEnemies)
            .ToList();
            List<SpawnableEnemyWithRarity> randomEnemies = new List<SpawnableEnemyWithRarity>();
            
            int randomNumber = UnityEngine.Random.Range(minNum.Value, maxNum.Value);
            allenemies = allenemies.OrderBy(x => UnityEngine.Random.value).ToList();
            randomEnemies.AddRange(allenemies.Take(randomNumber));
            
            foreach (var enemy in randomEnemies)
            {
                bool ins = false;

                if (bothInsideOutside.Value)
                {
                    ins = UnityEngine.Random.Range(0, 2) == 0;
                }
                else
                {
                    ins = isInside.Value;
                }

                Misc.SpawnEnemyForced(enemy, 1, ins);
            }
        }

        public static void Config()
        {
            minNum = MysteryDice.BepInExConfig.Bind<int>(
              "BlameGlitch",
              "Minimum Number of Enemies",
              4,
              "Minimum Number of Enemies to spawn from this event");
            maxNum = MysteryDice.BepInExConfig.Bind<int>(
              "BlameGlitch",
              "Max Number of Enemies",
              10,
              "Max Number of Enemies to spawn from this event");
            isInside = MysteryDice.BepInExConfig.Bind<bool>(
              "BlameGlitch",
              "Inside?",
              true,
              "Do they spawn inside");
            bothInsideOutside = MysteryDice.BepInExConfig.Bind<bool>(
              "BlameGlitch",
              "Both Inside and Outside",
              false,
              "Makes them spawn both inside and outside");

        }
    }
}
