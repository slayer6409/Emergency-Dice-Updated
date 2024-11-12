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
    internal class Fans : IEffect
    {
        public string Name => "Fans";

        public static int MaxMinesToSpawn = 5;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "I'm a big fan!";
        public void Use()
        {
            Networker.Instance.CustomTrapServerRPC(MaxMinesToSpawn ,GetEnemies.Fan.prefabToSpawn.name,false);
        }
    }
    
}
