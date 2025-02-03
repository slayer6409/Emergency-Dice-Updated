using System;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class GiveCredits : IEffect
    {
        public string Name => "More Money";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Credits Were Given!";
        public void Use()
        {
            
            Networker.Instance.AddMoneyServerRPC(UnityEngine.Random.Range(100, 400));
        }
    }
}
