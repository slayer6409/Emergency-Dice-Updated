using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Arachnophobia : IEffect
    {
        public string Name => "Arachnophobia";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spawns fast spiders";

        public static bool IsEnabled = false;
        public void Use()
        {
           Networker.Instance.ArachnophobiaServerRPC();
        }

        public static void SpawnSpiders()
        {
            Misc.SpawnEnemyForced(GetEnemies.Spider, 3, true);
        }
    }
}
