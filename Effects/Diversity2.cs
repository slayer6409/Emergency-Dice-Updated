using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using DiversityRemastered;

namespace MysteryDice.Effects
{
    internal class DiversityEffect2 : IEffect
    {
        public string Name => "Diversity Bracken";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Get Kidnapped";

        public void Use()
        {
            DiversityRemastered.Misc.StartOfRoundRevamp.Instance.KidnapServerRpc((ulong)Misc.GetRandomPlayerID());
        }
        public static bool checkConfigs()
        {
            if (DiversityRemastered.Configuration.brakenRevamp.Value != true) return false;
            return true;
        }
    }
}
