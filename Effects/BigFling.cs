using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class BigFling : IEffect
    {
        public string Name => "Big Fling";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "WEEEEEEEEEEEEEEEEEEEEEEEEEE";


        public void Use()
        {
            Networker.Instance.SpawnSurroundedServerRPC("Horse", 1, 2, true, new Vector3(10, 1.5f, 10));
        }

    }
}
