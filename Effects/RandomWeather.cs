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
    internal class RandomWeather : IEffect
    {
        public string Name => "Random Weather";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "New weather time!";

        public void Use()
        {
            List<string> weathers = WeatherRegistryCompat.getWeathers().ToList();
            Random rand = new Random();
            WeatherRegistryCompat.setWeather(weathers[rand.Next(0, weathers.Count)]);
        }
    }
}
