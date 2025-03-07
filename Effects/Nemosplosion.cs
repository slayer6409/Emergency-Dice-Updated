using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Nemosplosion : IEffect
    {
        public string Name => "Nemosplosion";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Awww sad";

        public void Use()
        {
            Networker.Instance.NemosplosionServerRPC();
        }
    }
}
