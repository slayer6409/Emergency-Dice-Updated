using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Unkillable : IEffect
    {
        public string Name => "Unkillable";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "YAY";

        public void Use()
        {
            Networker.Instance.UnkillableServerRpc(Misc.GetRandomPlayerID());
        }
    }
}
