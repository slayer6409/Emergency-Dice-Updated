using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BepInEx.Configuration;
using MysteryDice.Dice;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class AllAtOnce : IEffect
    {
        public string Name => "$All At Once";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "This is really bad";

        public void Use()
        {
            foreach (IEffect effect in DieBehaviour.AllowedEffects)
            {
                effect.Use();
            }
        }
    }
}
