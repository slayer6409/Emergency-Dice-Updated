﻿using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class TeleportInside : IEffect
    {
        public string Name => "Inside teleport";
        public EffectType Outcome => EffectType.Bad;
        public bool IsNegative { get { return true; } }
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Teleported inside";

        public void Use()
        {
            Vector3 randomInsidePos = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
            Networker.Instance.TeleportInsideServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController), randomInsidePos);
        }

        public static void TeleportPlayerInside(int clientID, Vector3 teleportPos)
        {
            PlayerControllerB player = null;

            player = Misc.GetPlayerByUserID(clientID);
            
            if (player == null) return;

            if ((bool)GameObject.FindObjectOfType<AudioReverbPresets>())
            {
                GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(player);
            }
            player.isInElevator = false;
            player.isInHangarShipRoom = false;
            player.isInsideFactory = true;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(teleportPos);
            player.beamOutParticle.Play();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }
    }
}
