using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BigJumpscare : IEffect
    {
        public string Name => "Big Jumpscare";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Just a bit of a jumpscare";


        public void Use()
        {
            Networker.Instance.SpawnSurroundedServerRPC("Scary", 10, 2, true, new Vector3(4, 4, 4));
        }

    }
}
