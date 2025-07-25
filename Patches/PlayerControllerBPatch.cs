﻿using System;
using HarmonyLib;
using MysteryDice.Effects;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine.InputSystem;
using LethalLib.Modules;
using Unity.Netcode;
using static LethalThings.DynamicBone.DynamicBoneColliderBase;
using LCTarrotCard.Util;
using LCTarrotCard;
using MysteryDice.MiscStuff;

namespace MysteryDice.Patches
{
    
    
    [HarmonyPatch(typeof(PlayerControllerB))]
    [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer), 
        typeof(int), typeof(bool), typeof(bool), typeof(CauseOfDeath), typeof(int), typeof(bool), typeof(Vector3))]
    class Patch_RemoveFallDamage
    {
        static bool Prefix(PlayerControllerB __instance, int damageNumber, bool hasDamageSFX, bool callRPC, CauseOfDeath causeOfDeath, int deathAnimation, bool fallDamage, Vector3 force)
        { 
            bool isFall = fallDamage || __instance.takingFallDamage || causeOfDeath == CauseOfDeath.Gravity;

            if (isFall && Fly.CanFly)
            {
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static bool HasInfiniteStamina = false;
        public static List<SmartAgentNavigator> smartAgentNavigators = new List<SmartAgentNavigator>();
        
        
        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("TeleportPlayer")]
        public class TeleportPlayerPatch
        {
            // static void Prefix(PlayerControllerB __instance, Vector3 pos, bool withRotation, float rot, bool allowInteractTrigger, bool enableController)
            // {
            //     foreach (SmartAgentNavigator navigator in smartAgentNavigators)
            //     {
            //         if (navigator != null)
            //         {
            //             navigator.positionsOfPlayersBeforeTeleport[__instance] = __instance.transform.position;
            //         }
            //     }
            // }
        }

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
        public static void CheckDoubleTap(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;

            float time = Time.time;

            if (time - Fly.lastTapTime > Fly.tapCooldown)
            {
                Fly.tapCount = 0;
            }

            bool spacePressed = MysteryDice.Keybinds.FlyButton.WasPressedThisFrame();
            if (!spacePressed) return;

            Fly.tapCount++;

            if (Fly.tapCount >= 2)
            {
                Fly.isFlying = !Fly.isFlying;
                Fly.tapCount = 0;
            }

            Fly.lastTapTime = time;
        }
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void CheckIfGrounded(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;

            if (__instance.IsPlayerNearGround() && Fly.isFlying)
                Fly.isFlying = false;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void FlyMode(PlayerControllerB __instance)
        {
            // if (!Fly.CanFly) return;
            //
            // if (MysteryDice.Keybinds.FlyButton.ReadValue<float>() > 0.5f)
            // {
            //     __instance.externalForces += Vector3.Lerp(__instance.externalForces, Vector3.ClampMagnitude(__instance.transform.up * 10, 400f), Time.deltaTime * 50f);
            //     __instance.fallValue = 0f;
            //     __instance.ResetFallGravity();
            // }
            
            if (!Fly.CanFly) return;
            if (!Fly.isFlying) return;

            var input = Keyboard.current;
            bool flyUp = MysteryDice.Keybinds.FlyButton.ReadValue<float>() > 0.5f;
            bool flyDown = MysteryDice.Keybinds.FlyDownButton.ReadValue<float>() > 0.5f;

            float verticalSpeed = 8f;
            Vector3 verticalVelocity = Vector3.zero;

            if (flyUp)
                verticalVelocity = __instance.transform.up * verticalSpeed;
            else if (flyDown)
                verticalVelocity = -__instance.transform.up * verticalSpeed;
            else
                verticalVelocity = Vector3.zero;

            __instance.externalForces = new Vector3(
                __instance.externalForces.x,
                verticalVelocity.y,
                __instance.externalForces.z
            );

            __instance.fallValue = 0f;
            __instance.ResetFallGravity();
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
            if (MysteryDice.useNeckBreakTimer.Value && !NeckBreak.isTimerRunning) Networker.Instance.StartCoroutine(NeckBreak.WaitTime());
        }
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void HyperShakeUpdate()
        {
            if(HyperShake.ShakingData == null) return;
            if(MysteryDice.hyperShakeTimer.Value > 0 && !HyperShake.isTimerRunning) Networker.Instance.StartCoroutine(HyperShake.WaitTime());
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


            if (MysteryDice.neckRotations.Value != -1)
            {
                if (MysteryDice.neckRotations.Value == NeckSpin.rotationNumber) 
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        public static void MartyrdomPlayer(PlayerControllerB __instance, int causeOfDeath, int deathAnimation, bool spawnBody, Vector3 bodyVelocity)
        {
            if(Martyrdom.doMinesDrop) Networker.Instance.doMartyrdomServerRPC(__instance.transform.position);
        }
    }
    
    [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
    public static class JumpPerformedPatch
    {
        [HarmonyPrefix]
        public static void JumpPerformedPrefix(InputAction.CallbackContext context, PlayerControllerB __instance)
        {
            if(!EggBoots.eggBootsEnabled) return;
            if(!EggBoots.canSpawnAnother) return;
            EggBoots.canSpawnAnother = false;
            Networker.Instance.spawnExplodeEggServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }
    }
    
}
