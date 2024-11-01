using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class DiversityEffect1 : IEffect
    {
        public string Name => "Stairs";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Down we go!";

        public void Use()
        {

            DiversityRemastered.Misc.StartOfRoundRevamp.Instance.TeleportToStairsServerRpc(Misc.GetRandomPlayerID());
        }
    }
}
