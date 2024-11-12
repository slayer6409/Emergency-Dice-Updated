// using GameNetcodeStuff;
// using LethalLib.Modules;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace MysteryDice.Effects
// {
//     internal class WhereDidMyFriendsGo : IEffect
//     {
//         public string Name => "Where Did My Friends Go?";
//         public EffectType Outcome => EffectType.Awful;
//         public bool ShowDefaultTooltip => true;
//         public string Tooltip => "Where did they go?";
//
//         public void Use()
//         {
//             Networker.Instance.WhereGoServerRPC(Misc.GetRandomPlayerID());
//         }
//
//         public static void whereTheyGo(ulong userID)
//         {
//             if (StartOfRound.Instance == null) return;
//             if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
//             if (StartOfRound.Instance.localPlayerController.playerClientId != userID) return;
//             foreach (var playerControllerB in StartOfRound.Instance.allPlayerScripts)
//             {
//
//                 if (playerControllerB != null && playerControllerB.actualClientId != StartOfRound.Instance.localPlayerController.actualClientId)
//                 {
//                     playerControllerB.gameObject.SetActive(false);
//                 }
//             }
//         }
//
//     }
// }
