using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class NutcrackerOutside : IEffect
    {
        public string Name => "21 Gun Salute";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "21 Gun Salute!";

        public void Use()
        {
            Networker.Instance.SpawnNutcrackerOutsideServerRPC();
        }

        public static void SpawnOutsideNutcracker()
        {
            if (GetEnemies.Nutcracker == null)
                return;
            int NutsToSpawn = UnityEngine.Random.Range(1, 6);

            RoundManager.Instance.currentOutsideEnemyPower = 0;
            Misc.SpawnEnemyForced(GetEnemies.Nutcracker, NutsToSpawn, false);
        }

    }
}
