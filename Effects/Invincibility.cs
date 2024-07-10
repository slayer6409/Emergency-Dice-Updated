using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Invincibility : IEffect
    {
        public string Name => "Invincibility";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Makes you a bit drunk";

        public static float MinVelocity = 10;

        public static float InvincibleTimer = 0f;

        public void Use()
        {
            Networker.Instance.InvincibleServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public static void FixedUpdate()
        {
            if (InvincibleTimer <= 0f) { returnValues(GameNetworkManager.Instance.localPlayerController.playerClientId); return; }
            if (InvincibleTimer > 0f)
            {
                InvincibleTimer -= Time.fixedDeltaTime;
                BecomeInvincible(GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
        }
        public static void InvincibilityStart(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            MinVelocity = player.minVelocityToTakeDamage;
            InvincibleTimer = UnityEngine.Random.Range(10, 30);
            
        }
        public static void returnValues(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            player.minVelocityToTakeDamage = MinVelocity;
        }
        public static void BecomeInvincible(ulong userID)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            if (player == null) return;
            player.minVelocityToTakeDamage = 100000;
            if (player.isPlayerDead) 
            {
                player.isPlayerDead = false;
                player.health = 1;
                if ((bool)GameObject.FindObjectOfType<AudioReverbPresets>())
                {
                    GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(player);
                }
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
                player.averageVelocity = 0f;
                player.velocityLastFrame = Vector3.zero;
                player.TeleportPlayer(StartOfRound.Instance.middleOfShipNode.position, withRotation: true, 160f);
                player.beamOutParticle.Play();
                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                }
            }
            else
            {
                player.health = 100;
            }
        }
    }
}
