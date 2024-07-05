using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class FakeFireExits : IEffect
    {
        public string Name => "Fake fire exits";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "More fire exits!";

        public void Use()
        {
            Networker.Instance.FakeFireExitsServerRPC();
        }

    }
}
