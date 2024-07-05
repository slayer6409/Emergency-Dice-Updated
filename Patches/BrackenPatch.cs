using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Patches
{
    //WIP

    /*
    [HarmonyPatch(typeof(FlowermanAI))]
    internal class BrackenPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PatchBrackenOwner(FlowermanAI __instance)
        {
            if (__instance.isEnemyDead)
                return;

                if (__instance.isOutside == true &&
                    __instance.OwnerClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
                        __instance.ChangeOwnershipOfEnemy(GameNetworkManager.Instance.localPlayerController.actualClientId);
        }

        //needed as the bracken doesnt kill connected players, only the host.
        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void PatchBrackenAttack(FlowermanAI __instance, ref Collider other, ref bool ___startingKillAnimationLocalClient)
        {
            if (__instance.isEnemyDead || !Networker.Instance.IsHost)
                return;

            PlayerControllerB player = other.GetComponent<PlayerControllerB>();

            if (!__instance.isOutside ||
                player == GameNetworkManager.Instance.localPlayerController ||
                player == null ||
                __instance.inKillAnimation)
                return;

            __instance.KillPlayerAnimationServerRpc((int)player.playerClientId);
            ___startingKillAnimationLocalClient = true;
            __instance.KillPlayerAnimationClientRpc((int)player.playerClientId);
        }
    }
    */
}
