using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using MysteryDice.Effects;
using UnityEngine;

namespace MysteryDice.Patches;

[HarmonyPatch(typeof(KillLocalPlayer), nameof(KillLocalPlayer.KillPlayer))]
public class KillLocalPlayerPatch
{
    static bool Prefix(KillLocalPlayer __instance, PlayerControllerB playerWhoTriggered)
    {
        if (__instance.gameObject.GetComponentInParent<RedPillChanger>() != null)
        {
            __instance.StartCoroutine(DamageOverTime(__instance,playerWhoTriggered));
            return false;
        }
        return true;
    }

    static IEnumerator DamageOverTime(KillLocalPlayer inst,PlayerControllerB player)
    {
        const int ticks = 5;
        const int dmgPerTick = 1;
        const float interval = 0.15f;

        for (int i = 0; i < ticks; i++)
        {
            if (player == null || player.isPlayerDead) yield break;
            player.DamagePlayer(dmgPerTick);
            yield return new WaitForSeconds(interval);
        }
        
        yield return new WaitForSeconds(0.1f);

        var enemyAI = inst.GetComponentInParent<EnemyAI>();
        var nodes = RoundManager.Instance.insideAINodes
            .Concat(RoundManager.Instance.outsideAINodes)
            .ToArray();
        if (RoundManager.Instance != null && nodes.Length > 0)
        {
            var randNode = nodes[Random.Range(0, nodes.Length)];
            enemyAI.agent.Warp(randNode.transform.position);
        }

        enemyAI.agent.speed = 3.5f;
    }
}