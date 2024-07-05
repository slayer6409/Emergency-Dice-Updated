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
    internal class Beepocalypse : IEffect
    {
        public string Name => "Beepocalypse";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Bee swarm";

        public void Use()
        {
            //Networker.Instance.DetonateRandomPlayerServerRpc();
            Networker.Instance.SpawnBeehivesServerRPC();
        }

        public static void SpawnBeehives()
        {
            for(int i = 0; i < UnityEngine.Random.Range(30,50); i++)
                Misc.SpawnOutsideEnemy(GetEnemies.Beehive);

            Networker.Instance.ZeroOutBeehiveScrapClientRPC();
        }

        public static void ZeroAllBeehiveScrap()
        {
            foreach (RedLocustBees enemy in GameObject.FindObjectsOfType<RedLocustBees>())
            {
                RedLocustBees bees = (RedLocustBees)enemy;
                if (bees)
                    if (bees.hive)
                        bees.hive.SetScrapValue(0);
            }
        }
    }
}
