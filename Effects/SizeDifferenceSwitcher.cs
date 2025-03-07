// using BepInEx.Configuration;
// using GameNetcodeStuff;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
//
// namespace MysteryDice.Effects
// {
//     internal class SizeDifferenceSwitcher : IEffect
//     {
//         public string Name => "Size Switcher";
//         public EffectType Outcome => EffectType.Good;
//         public bool ShowDefaultTooltip => false;
//         public static bool canSwitch = false; 
//         public string Tooltip => "Everyone can freely switch size now! /Size to swap and /visor to toggle visor";
//
//         public void Use()
//         {
//             Networker.Instance.SizeSwitcherServerRPC();
//         }
//         public static void BecomeSmall(ulong userID)
//         {
//             PlayerControllerB player = Misc.GetPlayerByUserID(userID);
//             if (player == null) return;
//             if(player.transform.localScale != Vector3.one)
//             {
//                 Networker.Instance.fixSizeServerRPC(userID);
//                 return;
//             }
//             player.transform.localScale = new Vector3(0.3333f, 0.33333f, 0.333333f);
//         }
//         public static void StartSwitcher()
//         {
//             Misc.SafeTipMessage("You can now Switch Sizes", "Do /size to swap and /visor to toggle your visor");
//             canSwitch = true;
//         }
//     }
// }
