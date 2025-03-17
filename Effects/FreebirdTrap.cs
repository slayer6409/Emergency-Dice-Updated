using MysteryDice.Patches;
using System;
using System.Collections;
using CodeRebirth.src.Content.Enemies;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class FreebirdTrap : IEffect
    {
        public string Name => "Freebird Trap";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "FREEBIRDDDDDDDDDDDDDDDDDDDDDD YEAH";
        
        public void Use()
        {
            Networker.Instance.SpawnFreebirdTrapServerRPC("landmine", true);
        }
    }
}
