using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Ghosts : IEffect
    {
        public string Name => "The Shining";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Come and play with us. Forever... and ever... and ever";

        public void Use()
        {
            Networker.Instance.SpawnGhostsServerRPC();
        }

        public static void SpawnGhosts()
        {
            if (GetEnemies.Ghost == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Ghost, 2, true);

        }

    }
}
