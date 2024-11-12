using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class BurgerFlippers : IEffect
    {
        public string Name => "Burger Flippers";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "WEEEEEEEEEEE";

        public void Use()
        {
            Networker.Instance.SpawnSurroundedServerRPC(GetEnemies.Horse.enemyType.enemyName, 12);
        }

    }
}
