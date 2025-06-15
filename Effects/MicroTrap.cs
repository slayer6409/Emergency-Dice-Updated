using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class MicroTrap : IEffect
    {
        public string Name => "Micro Trap";

        public static int MaxMinesToSpawn = 5;

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "It so Smol";
        public void Use()
        {
            var traps = Misc.getAllTraps();
            Networker.Instance.CustomScaledTrapServerRPC(MaxMinesToSpawn , traps[Random.Range(0, traps.Length)].name ,Random.value>0.5f, new Vector3(0.3f,0.3f,.3f), false);
        }
    }
    
}
