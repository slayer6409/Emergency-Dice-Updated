using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Papercut : IEffect
    {
        public string Name => "Papercut";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "They might cut you";

        public void Use()
        {
            Networker.Instance.paperCutServerRPC();
        }
    } 
}
