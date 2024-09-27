using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Meteors : IEffect
    {
        public string Name => "Meteors";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "I hope you like space rocks";

        public static bool isRunning = false;
        public void Use()
        {
            Networker.Instance.SpawnMeteorsServerRPC();
        }
    }
}
