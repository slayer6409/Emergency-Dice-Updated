using System;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class BigDelivery : IEffect
    {
        public string Name => "Big Delivery";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Dropship inbound";
        public void Use()
        {
            Networker.Instance.BigDeliveryServerRPC();
        }
    }
}
