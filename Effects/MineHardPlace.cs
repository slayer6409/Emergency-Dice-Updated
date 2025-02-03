using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class MineHardPlace : IEffect
    {
        public string Name => "Mine and a Hard Place";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can't go anywhere now";

        public void Use()
        {
            Networker.Instance.SpawnSurroundedTrapServerRPC(GetEnemies.Bertha.prefabToSpawn.name,8,7,true,new Vector3(1.25f,2f,1.25f));
        }
        
    }
}
