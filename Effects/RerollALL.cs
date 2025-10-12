using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class RerollALL : GalEffect
    {
        public string Name => "RerollALL";
        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Try Again";

        public void Use()
        {
            Networker.Instance.RerollAllServerRPC();
        }
    }
}
