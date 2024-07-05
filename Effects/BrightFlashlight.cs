using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BrightFlashlight : IEffect
    {
        public string Name => "Bright Flashlight";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Makes every flashlight brighter";

        public static bool IsEnabled = false;

        public void Use()
        {
            Networker.Instance.FlashbrightServerRPC();
        }

    }
}
