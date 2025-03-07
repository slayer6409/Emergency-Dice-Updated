using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BepInEx.Configuration;
using MysteryDice.Dice;
using MysteryDice.Patches;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class AddWeather : IEffect
    {
        public string Name => "Add Weather";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Adds a weather to the day";

        public void Use()
        {
            List<string> allWeathers = WeatherRegistryCompat.getWeathers().ToList();
            Random rand = new Random();
            WeatherRegistryCompat.addWeather(allWeathers[rand.Next(0, allWeathers.Count)]);
            
        }
    }
}
