using HarmonyLib;
using LethalLib;
using MysteryDice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartRoundManager(RoundManager __instance)
        {
            
        }

        public static List<Vector3> MapObjectsPositions = new List<Vector3>();

        [HarmonyPrefix]
        [HarmonyPatch("SpawnMapObjects")]
        public static void SpawnMapObjectsPatch(RoundManager __instance)
        {
            if (__instance.currentLevel.spawnableMapObjects.Length == 0)
                return;

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 587);
            __instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            RandomMapObject[] array = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            foreach (RandomMapObject randomMapObj in array)
                MapObjectsPositions.Add(randomMapObj.transform.position);


        }

    }
}
