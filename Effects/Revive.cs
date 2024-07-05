using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Revive : IEffect
    {
        public string Name => "Revive";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Reviving everyone";
        public void Use()
        {
            Networker.Instance.ReviveAllPlayersServerRpc();
        }
    }
}
