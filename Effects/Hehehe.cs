using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Hehehe : IEffect
    {
        public string Name => "Hehehe";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Hehehe";
        public static GameObject spiderCanvas;

        public void Use()
        {
            Networker.Instance.HeheheServerRpc(Misc.GetRandomPlayerID());
        }
    }
}
