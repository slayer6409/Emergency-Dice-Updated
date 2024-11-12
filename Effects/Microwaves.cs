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
    internal class Microwave : IEffect
    {
        public string Name => "Microwave";

        public static int MaxMinesToSpawn = 7;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Who put microwaves outside?";
        public void Use()
        {
            Networker.Instance.CustomTrapServerRPC(MaxMinesToSpawn,GetEnemies.Microwave.prefabToSpawn.name,false);
        }
    }
    
}
