using HarmonyLib;
using MysteryDice.Effects;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Windows;
using System.Collections;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static bool HasInfiniteStamina = false;

        [HarmonyPrefix]
        [HarmonyPatch("HasLineOfSightToPosition")]
        public static bool OverrideLineOfSight(ref bool __result)
        {
            if(RebeliousCoilHeads.IsEnabled && Networker.CoilheadIgnoreStares)
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void InfiniteSprint(ref float ___sprintMeter)
        {
            if (HasInfiniteStamina)
                ___sprintMeter = 1f;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void FlyMode(PlayerControllerB __instance)
        {
            if (!Fly.CanFly) return;

            if (IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>() > 0.5f)
            {
                __instance.externalForces += Vector3.Lerp(__instance.externalForces, Vector3.ClampMagnitude(__instance.transform.up * 10, 400f), Time.deltaTime * 50f);
                __instance.fallValue = 0f;
                __instance.ResetFallGravity();
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void BreakNeckUpdate(PlayerControllerB __instance)
        {
            if (NeckBreak.IsNeckBroken == 0) return;
            
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;


            switch (NeckBreak.IsNeckBroken) 
            {
                case 0: return;
                case 1: cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 90f); break;
                case 2: cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 180f); break;
                case 3: cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, -90f); break;
                case 4: cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, 0f); NeckBreak.IsNeckBroken = 0; break;
                default: NeckBreak.IsNeckBroken = 0; break;

            }
            if (NeckBreak.useTimer && !NeckBreak.isTimerRunning) Networker.Instance.StartCoroutine(NeckBreak.WaitTime());
        }
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void NeckSpinUpdate(PlayerControllerB __instance)
        {
            if (NeckSpin.IsNeckSpinning == 0) return;
            Transform cam = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
            if (__instance.isClimbingLadder)
            {
                NeckSpin.counter = 0;
                NeckSpin.savedValue = cam.eulerAngles.z;
                NeckSpin.wasClimbing = true;
                return;
            }
            else if (NeckSpin.wasClimbing)
            {
                if (NeckSpin.counter >= 50)
                {
                    NeckSpin.wasClimbing = false;
                }
                NeckSpin.counter++;
                cam.eulerAngles = new Vector3(cam.eulerAngles.x, cam.eulerAngles.y, NeckSpin.savedValue);
                return;
            }
            cam.eulerAngles += new Vector3(0, 0, NeckSpin.neckChoiceSpeed * Time.deltaTime);


            if (NeckSpin.numberOfRotations != -1)
            {
                if (NeckSpin.numberOfRotations == NeckSpin.rotationNumber) 
                {
                    NeckSpin.FixNeck();
                    NeckSpin.IsNeckSpinning = 0;
                }
                if (cam.eulerAngles.z >= 359)
                {
                    NeckSpin.rotationNumber++;
                    cam.eulerAngles = new Vector3(cam.eulerAngles.x,cam.eulerAngles.y,0);
                }
            }
        }
    }
}
