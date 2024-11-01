using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class BigBees : IEffect
    {
        public string Name => "Big Bees";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Ya Like Jazz";

        public void Use()
        {
            //Networker.Instance.DetonateRandomPlayerServerRpc();
            Networker.Instance.SpawnBigBeehivesServerRPC();
        }

        public static void SpawnBeehives()
        {
            var bees = Misc.SpawnOutsideEnemy(GetEnemies.Beehive, true);
            bees.transform.localScale = new Vector3(10, 10, 10);
        }

    }
}
