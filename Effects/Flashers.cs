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
    internal class Flashers : IEffect
    {
        public string Name => "Flashers";

        public static int MaxMinesToSpawn = 5;

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "They gonna flash ya!";
        public void Use()
        {
            Networker.Instance.CustomTrapServerRPC(MaxMinesToSpawn,GetEnemies.FlashTurret.prefabToSpawn.name,false);
        }
    }
    
}
