using System;
using System.Linq;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Revive : IEffect
    {
        public string Name => "Revive";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Reviving everyone";
        public static int lives = 0;
        public void Use()
        {
	        foreach (var player in StartOfRound.Instance.allPlayerScripts)
	        {
		        if(player.isPlayerDead) Networker.Instance.RevivePlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), StartOfRound.Instance.middleOfShipNode.position);
	        }
        }
        public static void revivePlayer(int playerID, Vector3 SpawnPosition)
        {
	        int DefaultHealth = 100;
	        var playerControllerB = StartOfRound.Instance.allPlayerScripts[playerID];
	        //if ( playerControllerB.isPlayerDead)
			if (!playerControllerB.isPlayerDead) return;
			playerControllerB.isInsideFactory = false;
			playerControllerB.isInHangarShipRoom = true;
			playerControllerB.ResetPlayerBloodObjects(playerControllerB.isPlayerDead);
			playerControllerB.health = DefaultHealth;
			playerControllerB.isClimbingLadder = false;
			playerControllerB.clampLooking = false;
			playerControllerB.inVehicleAnimation = false;
			playerControllerB.disableMoveInput = false;
			playerControllerB.disableLookInput = false;
			playerControllerB.disableInteract = false;
			playerControllerB.ResetZAndXRotation();
			playerControllerB.thisController.enabled = true;
			if (playerControllerB.isPlayerDead)
			{
				MonoBehaviour.print("player is dead, reviving them.");
				playerControllerB.thisController.enabled = true;
				playerControllerB.isPlayerDead = false;
				playerControllerB.isPlayerControlled = true;
				playerControllerB.health = DefaultHealth;
				playerControllerB.hasBeenCriticallyInjured = false;
				playerControllerB.criticallyInjured = false;
				playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
				playerControllerB.TeleportPlayer(SpawnPosition);
				playerControllerB.parentedToElevatorLastFrame = false;
				playerControllerB.overrideGameOverSpectatePivot = null;
				StartOfRound.Instance.SetPlayerObjectExtrapolate(enable: false);
				playerControllerB.setPositionOfDeadPlayer = false;
				playerControllerB.DisablePlayerModel(playerControllerB.gameObject, enable: true, disableLocalArms: true);
				playerControllerB.helmetLight.enabled = false;
				playerControllerB.Crouch(crouch: false);
				if (playerControllerB.playerBodyAnimator != null)
				{
					playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
				}
				playerControllerB.bleedingHeavily = false;
				if (playerControllerB.deadBody != null)
				{
					playerControllerB.deadBody.enabled=false;
					playerControllerB.deadBody.gameObject.SetActive(false);
				}
				playerControllerB.deadBody = null;
				playerControllerB.activatingItem = false;
				playerControllerB.twoHanded = false;
				playerControllerB.inShockingMinigame = false;
				playerControllerB.inSpecialInteractAnimation = false;
				playerControllerB.freeRotationInInteractAnimation = false;
				playerControllerB.disableSyncInAnimation = false;
				playerControllerB.inAnimationWithEnemy = null;
				playerControllerB.holdingWalkieTalkie = false;
				playerControllerB.speakingToWalkieTalkie = false;
				playerControllerB.isSinking = false;
				playerControllerB.isUnderwater = false;
				playerControllerB.sinkingValue = 0f;
				playerControllerB.statusEffectAudio.Stop();
				playerControllerB.DisableJetpackControlsLocally();
				playerControllerB.mapRadarDotAnimator.SetBool("dead", value: false);
				playerControllerB.hasBegunSpectating = false;
				playerControllerB.externalForceAutoFade = Vector3.zero;
				playerControllerB.hinderedMultiplier = 1f;
				playerControllerB.isMovementHindered = 0;
				playerControllerB.sourcesCausingSinking = 0;
				playerControllerB.reverbPreset = StartOfRound.Instance.shipReverb;
				SoundManager.Instance.earsRingingTimer = 0f;
				playerControllerB.voiceMuffledByEnemy = false;
				SoundManager.Instance.playerVoicePitchTargets[playerID] = 1f;
				SoundManager.Instance.SetPlayerPitch(1f, playerID);
				if (playerControllerB.currentVoiceChatIngameSettings == null)
				{
					StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
				}
				if (playerControllerB.currentVoiceChatIngameSettings != null)
				{
					if (playerControllerB.currentVoiceChatIngameSettings.voiceAudio == null)
					{
						playerControllerB.currentVoiceChatIngameSettings.InitializeComponents();
					}
					if (playerControllerB.currentVoiceChatIngameSettings.voiceAudio == null)
					{
						return;
					}
					playerControllerB.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
				}

				if (playerControllerB.isPlayerDead)
				{
					HUDManager.Instance.UpdateBoxesSpectateUI();
					HUDManager.Instance.UpdateSpectateBoxSpeakerIcons();
				}
			}
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			if (localPlayerController == playerControllerB)
			{
				localPlayerController.bleedingHeavily = false;
				localPlayerController.criticallyInjured = false;
				localPlayerController.health = DefaultHealth;
				HUDManager.Instance.UpdateHealthUI(DefaultHealth);
				localPlayerController.playerBodyAnimator.SetBool("Limp", value: false);
				localPlayerController.spectatedPlayerScript = null;
				StartOfRound.Instance.SetSpectateCameraToGameOverMode(enableGameOver: false, localPlayerController);
				StartOfRound.Instance.SetPlayerObjectExtrapolate(enable: false);
				HUDManager.Instance.audioListenerLowPass.enabled = false;
				HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
				HUDManager.Instance.RemoveSpectateUI();
				HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
			}
			StartOfRound.Instance.allPlayersDead = false;
			StartOfRound.Instance.livingPlayers++;
			StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }
    }
}
