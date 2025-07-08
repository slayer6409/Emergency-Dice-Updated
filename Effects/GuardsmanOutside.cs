using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class GuardsmanOutside : IEffect
    {
        public string Name => "GuardsmanOutside";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Just a phone call away!";

        public void Use()
        {
            Networker.Instance.CustomMonsterServerRPC("Guardsman", 3, 5, false);
        }


    }
}
