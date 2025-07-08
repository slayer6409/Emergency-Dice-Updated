using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using MysteryDice.CompatThings;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class HappyFamily : IEffect
    {
        public string Name => "Happy Family";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "One Big Happy Family!";
        
        public void Use()
        {
            Networker.Instance.CustomMonsterServerRPC("Mistress", 1, 1, true);
            Networker.Instance.CustomMonsterServerRPC("Lord Of The Manor", 1, 1, true);
            Networker.Instance.CustomMonsterServerRPC("Rabbit Magician", 1, 1, true);
            Networker.Instance.CustomMonsterServerRPC("Girl", 1, 1, true);
        }
    }
}
