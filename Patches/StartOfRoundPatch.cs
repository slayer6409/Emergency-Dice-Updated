using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using LethalLib.Modules;
using MysteryDice.Dice;
using MysteryDice.Effects;
using MysteryDice.Visual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void InstantiateNetworker(StartOfRound __instance)
        {
            MysteryDice.JumpscareOBJ = GameObject.Instantiate(MysteryDice.JumpscareCanvasPrefab);
            MysteryDice.JumpscareScript = MysteryDice.JumpscareOBJ.GetComponent<Jumpscare>();
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (Networker.Instance == null)
                {
                    GameObject go = GameObject.Instantiate(MysteryDice.NetworkerPrefab,Vector3.zero,Quaternion.identity);
                    go.GetComponent<NetworkObject>().Spawn(false);
                }
                else
                {
                    MysteryDice.CustomLogger.LogError("Networker already exists??????");
                }
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        public static void OnStartGame(StartOfRound __instance)
        {
            GetEnemies.allEnemies = Resources.FindObjectsOfTypeAll<EnemyType>().ToList();
            if (!Networker.Instance.IsServer) return;

            if(MysteryDice.LoversOnStart.Value && StartOfRound.Instance.IsHost) new Lovers().Use();
            Networker.Instance.OnStartRoundClientRPC();
            ResetSettingsShared();
        }
        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        public static void BrutalStart(StartOfRound __instance)
        {
            if (!Networker.Instance.IsServer) return;
            if (!MysteryDice.BrutalMode.Value) return;
            int EventsToBe = Networker.Instance.CheckScaling();
            for (int i = 0; i < EventsToBe; i++)
            {
                if (MysteryDice.SuperBrutalMode.Value)
                {
                    StartOfRound.Instance.StartCoroutine(waitForBrutal());
                }
                else
                {
                    Networker.Instance.QueueRandomDiceEffectServerRPC("Brutal");
                }
            }
        }

        public static IEnumerator waitForBrutal()
        {
            yield return new WaitForSeconds(Random.Range(30f, 600f));
            Networker.Instance.QueueRandomDiceEffectServerRPC("Brutal");
        }

        public static void StartGameOnClient()
        {
            if (Networker.Instance.IsServer) return;

            ResetSettingsShared();
        }

        [HarmonyPostfix]
        [HarmonyPatch("EndGameClientRpc")]
        public static void OnEndGameClient(StartOfRound __instance)
        {
            ResetSettingsShared();
            Lovers.removeLovers();
        }

        [HarmonyPostfix]
        [HarmonyPatch("EndGameServerRpc")]
        public static void OnEndGameServer(StartOfRound __instance)
        {
            ResetSettingsShared();
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShipHasLeft")]
        public static void OnEndShipHasLeft(StartOfRound __instance)
        {
            ResetSettingsShared();
        }


        public static void ResetSettingsShared()
        {
            FireExitPatch.AreFireExitsBlocked = false;
            Networker.CoilheadIgnoreStares = false;
            RebeliousCoilHeads.IsEnabled = false;
            TerminalPatch.hideShowTerminal(false,01);
            Arachnophobia.IsEnabled = false;
            ModifyPitch.ResetPitch();
            Armageddon.IsEnabled = false;
            AlarmCurse.IsCursed = false;
            TurretPatch.FastCharging = false;
            BrightFlashlight.IsEnabled = false;
            PlayerControllerBPatch.HasInfiniteStamina = false;
            HyperShake.ShakingData = null;
            EggBoots.eggBootsEnabled = false;
            Martyrdom.doMinesDrop = false;
            
            
            if (LeverShake.IsShaking)
            {
                LeverShake.ShipLeverTrigger.transform.localPosition = LeverShake.InitialLevelTriggerLocalPosition;
                LeverShake.ShipLever.transform.localPosition = LeverShake.InitialLevelTriggerLocalPosition;
            }
          
            LeverShake.IsShaking = false;
            LeverShake.ShipLeverTrigger = null;

            Fly.CanFly = false;

            SelectEffect.CloseSelectMenu();

            NeckBreak.FixNeck();
            
            // if (!MysteryDice.DisableSizeBased.Value)
            // {
            //     if (SizeDifferenceSwitcher.canSwitch)
            //     {
            //         Networker.Instance.fixSizeServerRPC(StartOfRound.Instance.localPlayerController.actualClientId);
            //         SizeDifferenceSwitcher.canSwitch = false;
            //     }
            // }
            
            NeckSpin.FixNeck();
            TimeOfDay.Instance.overrideMeteorChance = -1;
            TimeOfDay.Instance.meteorShowerAtTime = -1;
            Meteors.isRunning = false;

            // if (!MysteryDice.DisableSizeBased.Value)
            // {
            //     if (SizeDifference.sizeOption.Value == SizeDifference.sizeRevert.after || SizeDifference.sizeOption.Value == SizeDifference.sizeRevert.bothAgainAfter) Networker.Instance.fixSizeServerRPC(StartOfRound.Instance.localPlayerController.actualClientId);
            // }
            //if (Networker.Instance == null) MysteryDice.CustomLogger.LogFatal("Networker is null, this should never happen, what is going on???????");
            //else
            //{
            
            Networker.Instance.StopAllCoroutines();
            //}

            if (Networker.Instance.IsServer)
                Networker.Instance.SyncRateClientRPC(StartOfRound.Instance.companyBuyingRate);
        }
    }
}
