using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class RemoveCredits : IEffect
    {
        public string Name => "Stolen Credits";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Who stole our credits?";
        public void Use()
        {
            Networker.Instance.AddMoneyServerRPC(UnityEngine.Random.Range(-100, -300));
        }
    }
}
