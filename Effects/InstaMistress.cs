using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class ManyMistress : IEffect
    {
        public string Name => "ManyMistress";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Head Choppin Time";

        public void Use()
        {
            Networker.Instance.spawnMistressServerRPC();
            
        }

        public static void SpawnMistress()
        {
            if (!Networker.Instance.IsHost) return;
            
            
            Misc.SpawnEnemyForced2(Misc.getEnemyByName("Mistress"),4, false);
        }
        

       
    }
}
