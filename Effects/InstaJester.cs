using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class InstaJester : IEffect
    {
        public string Name => "Instajester";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spawned a popped jester!";

        public void Use()
        {
            Networker.Instance.InstaJesterServerRPC();
        }

        public static void SpawnInstaJester()
        {
            if (GetEnemies.Jester == null)
                return;

            Misc.SpawnEnemyForced(GetEnemies.Jester, 1, true);

            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (enemy is JesterAI)
                {
                    JesterAI jester = (JesterAI)enemy;
                    jester.SwitchToBehaviourState(1);
                    jester.StartCoroutine(DelayPop(jester));
                }
            }
        }

        //hacky fix as the jester switched states, but didnt pop out visually
        public static IEnumerator DelayPop(JesterAI jester)
        {
            yield return new WaitForSeconds(1);
            jester.SwitchToBehaviourState(2);
        }
    }
}
