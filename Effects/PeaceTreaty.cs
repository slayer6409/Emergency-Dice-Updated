using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class PeaceTreaty : IEffect
    {
        public string Name => "PeaceTreaty";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Peace treaty";

        public void Use()
        {
            Networker.Instance.CustomMonsterServerRPC("Peace Keeper", 3, 5, false);
        }
    }
}