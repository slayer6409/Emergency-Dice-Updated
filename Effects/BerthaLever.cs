using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BerthaLever : IEffect
    {
        public string Name => "Bertha Lever";
        public static int MinMinesToSpawn = 1;
        public static int MaxMinesToSpawn = 1;

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Just try and leave, I dare you";
        public static Vector3 spawnPosition = Vector3.zero;
        public static GameObject ShipLeverTrigger = null;
        public void Use()
        {
            Networker.Instance.BerthaOnLeverServerRPC();
        }

        public static void SpawnBerthaOnLever()
        {
            spawnPosition = GameObject.Find("StartGameLever").transform.position;
            GameObject gameObject = UnityEngine.Object.Instantiate(
                GetEnemies.Bertha.prefabToSpawn,
                spawnPosition,
                Quaternion.identity,
                RoundManager.Instance.mapPropsContainer.transform);
            gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        }

        
    }
    
}
