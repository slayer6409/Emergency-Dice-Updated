using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TerminalLockout : IEffect
    {
        public string Name => "Terminal Lockout";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Where did the terminal go?";

        public static float DrunkTimer = 0f;

        public void Use()
        {
            //Networker.Instance.TerminalLockoutServerRPC();
        }
    }
}
