using HarmonyLib;
using LethalLib;
using LethalLib.Modules;
using MysteryDice.Effects;
using MysteryDice.Visual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

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
                GameObject go = GameObject.Instantiate(MysteryDice.NetworkerPrefab,Vector3.zero,Quaternion.identity);
                go.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        public static void OnStartGame(StartOfRound __instance)
        {
            if (!Networker.Instance.IsServer) return;

            Networker.Instance.OnStartRoundClientRPC();
            ResetSettingsShared();
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
            Arachnophobia.IsEnabled = false;
            ModifyPitch.ResetPitch();
            Armageddon.IsEnabled = false;
            AlarmCurse.IsCursed = false;
            TurretPatch.FastCharging = false;
            BrightFlashlight.IsEnabled = false;
            PlayerControllerBPatch.HasInfiniteStamina = false;
            HyperShake.ShakingData = null;

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
               
            Networker.Instance.StopAllCoroutines();

            if (Networker.Instance.IsServer)
                Networker.Instance.SyncRateClientRPC(StartOfRound.Instance.companyBuyingRate);
        }

    }
}
