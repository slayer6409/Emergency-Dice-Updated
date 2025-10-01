using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Revive : IEffect
    {
        public string Name => "Revive All";
        public EffectType Outcome => EffectType.GalGreat;
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
				HUDManager.Instance.HideHUD(false);
			}
			StartOfRound.Instance.allPlayersDead = false;
			StartOfRound.Instance.livingPlayers++;
			StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }

        public static int CountDeadPlayers()
        {
	        var arr = StartOfRound.Instance.allPlayerScripts;
	        int c = 0;
	        for (int i = 0; i < arr.Length; i++)
		        if (arr[i].isPlayerDead) c++;
	        return c;
        }

        public static PlayerControllerB getRandomDeadPlayer()
        {
	        List<PlayerControllerB> players = new();
	        foreach (var player in StartOfRound.Instance.allPlayerScripts)
	        {
		        if (player.isPlayerDead) players.Add(player);
	        }
	        if (players.Count == 0) return null;
	        else return players[UnityEngine.Random.Range(0, players.Count)];
        }
        
        public static IEnumerator reviveNext()
        {
	        yield return new WaitUntil(() => CountDeadPlayers() != 0);
	        var player = getRandomDeadPlayer();
	        Networker.Instance.RevivePlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player), StartOfRound.Instance.middleOfShipNode.position);
        }
    }

    internal class ReviveNext : IEffect
    {
	    public string Name => "Revive Next";
	    public EffectType Outcome => EffectType.Good;
	    public bool ShowDefaultTooltip => false;
	    public string Tooltip => "One Revive";
	    public void Use()
	    {
		    Networker.Instance.reviveNextServerRPC();
	    }
    }
    internal class RevivePercent : IEffect
    {
	    public string Name => "Revive Percent";
	    public EffectType Outcome => EffectType.Great;
	    public bool ShowDefaultTooltip => false;
	    public string Tooltip => "Maybe Revives";
	    public void Use()
	    {
		    var percent = Random.Range(0.01f, 0.65f);
		    Networker.Instance.reviveChanceServerRPC(percent);
	    }
    }
}
