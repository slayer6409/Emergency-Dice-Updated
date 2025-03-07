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
    internal class ClearWeather : IEffect
    {
        public string Name => "Clear Weather";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Sunshiny Day";

        public void Use()
        {
              WeatherRegistryCompat.setWeather("None");
        }
    }
}
