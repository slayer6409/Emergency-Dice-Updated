// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using GameNetcodeStuff;
// using MysteryDice.Dice;
// using Unity.Netcode;
// using Unity.Netcode.Components;
// using UnityEngine;
// using UnityEngine.AI;
//
// namespace MysteryDice.Gal;
//
// public class DiceGalAI : GalAI
// {public SkinnedMeshRenderer FaceSkinnedMeshRenderer = null!;
//     
//     public GameObject boostersGameObject = null!;
//     public InteractTrigger keyboardInteractTrigger = null!;
//     public InteractTrigger zapperInteractTrigger = null!;
//     public InteractTrigger keyInteractTrigger = null!;
//     public InteractTrigger teleporterInteractTrigger = null!;
//     public AudioSource FlyingSource = null!;
//     public AudioSource FootstepSource = null!;
//     public AudioSource specialSource = null!;
//     public AudioClip scrapPingSound = null!;
//     public AudioClip keyboardPressSound = null!;
//     public AudioClip zapperSound = null!;
//     public AudioClip keySound = null!;
//     public AudioClip teleporterSound = null!;
//     public List<AudioClip> startOrEndFlyingAudioClips = new();
//
//     private Collider[] cachedColliders = new Collider[5];
//     private List<Coroutine> customPassRoutines = new();
//     private List<GameObject> pointsOfInterest = new();
//     private float scrapRevealTimer = 10f;
//     private bool flying = false;
//     private Coroutine? unlockingSomething = null;
//     private Coroutine? zapperRoutine = null;
//     private State galState = State.Inactive;
//     [HideInInspector] public Emotion galEmotion = Emotion.Sleeping;
//     private readonly static int revealScrapAnimation = Animator.StringToHash("revealScrap"); // trigger
//     private readonly static int flyingAnimation = Animator.StringToHash("flying"); // bool
//     private readonly static int danceAnimation = Animator.StringToHash("dancing"); // bool
//     private readonly static int activatedAnimation = Animator.StringToHash("activated"); // bool
//     private readonly static int runSpeedFloat = Animator.StringToHash("RunSpeed"); // float
//     private readonly static int specialAnimationInt = Animator.StringToHash("specialAnimationInt"); // int (-1 is nothing, 0 is DoorUnlock, 1 is RechargeItem, 2 is Teleport).
//
//     public enum State
//     {
//         Inactive = 0,
//         Active = 1,
//         FollowingPlayer = 2,
//         Dancing = 3,
//         UnlockingObjects = 4,
//     }
//
//     public enum Emotion
//     {
//         Basis = -1,
//         VeryHappy = 0,
//         Mood = 1,
//         Angy = 2,
//         Winky = 3,
//         Crying = 4,
//         Sleeping = 5,
//         Flustered = 6,
//         Love = 7,
//     }
//
//     private void StartUpDelay()
//     {
//         List<DiceCharger> diceChargers = new();
//         foreach (var charger in Charger.Instances)
//         {
//             if (charger is DiceCharger actuallyADiceCharger)
//             {
//                 diceChargers.Add(actuallyADiceCharger);
//             }
//         }
//         if (diceChargers.Count <= 0)
//         {
//             if (IsServer) NetworkObject.Despawn();
//             //Plugin.Logger.LogError($"TerminalCharger not found in scene. TerminalGalAI will not be functional.");
//             return;
//         }
//         DiceCharger diceCharger = diceChargers.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First(); ;
//         diceCharger.GalAI = this;
//         GalCharger = diceCharger;
//         // Automatic activation if configured
//         if (MysteryDice.ConfigGalAutomatic.Value)
//         {
//             StartCoroutine(GalCharger.ActivateGalAfterLand());
//         }
//
//         // Adding listener for interaction trigger
//         GalCharger.ActivateOrDeactivateTrigger.onInteract.AddListener(GalCharger.OnActivateGal);
//         keyboardInteractTrigger.onInteract.AddListener(OnKeyboardInteract);
//         keyInteractTrigger.onInteract.AddListener(OnKeyHandInteract);
//         zapperInteractTrigger.onInteract.AddListener(OnZapperInteract);
//         teleporterInteractTrigger.onInteract.AddListener(OnTeleporterInteract);
//
//         foreach (var item in StartOfRound.Instance.allItemsList.itemsList)
//         {
//             if (item == null || item.spawnPrefab == null || !item.spawnPrefab.TryGetComponent(out GrabbableObject grabbableObject)) continue;
//             if (grabbableObject.mainObjectRenderer != null && grabbableObject.mainObjectRenderer.gameObject.layer == 0)
//             {
//                 grabbableObject.mainObjectRenderer.gameObject.layer = 6;
//             }
//             else
//             {
//                 foreach (var renderer in grabbableObject.GetComponentsInChildren<Renderer>())
//                 {
//                     if (renderer.gameObject.layer == 0)
//                     {
//                         renderer.gameObject.layer = 6;
//                     }
//                 }
//             }
//         }
//     }
//
//
//     private void OnZapperInteract(PlayerControllerB playerInteracting)
//     {
//         if (zapperRoutine != null || !playerInteracting.IsLocalPlayer || playerInteracting != ownerPlayer) return;
//         ZapperInteractServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, playerInteracting));
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     private void ZapperInteractServerRpc(int playerIndex)
//     {
//         ZapperInteractClientRpc(playerIndex);
//     }
//
//     [ClientRpc]
//     private void ZapperInteractClientRpc(int playerIndex)
//     {
//         zapperRoutine = StartCoroutine(RechargePlayerHeldEquipment(StartOfRound.Instance.allPlayerScripts[playerIndex]));
//     }
//
//     private IEnumerator RechargePlayerHeldEquipment(PlayerControllerB playerToRecharge)
//     {
//         if (playerToRecharge.isPlayerDead || !playerToRecharge.isPlayerControlled || playerToRecharge.currentlyHeldObjectServer == null || !playerToRecharge.currentlyHeldObjectServer.itemProperties.requiresBattery)
//         {
//             if (playerToRecharge.IsLocalPlayer) HUDManager.Instance.DisplayTip("Error", "What you're holding cannot be charged", false);
//             yield break;
//         }
//         else
//         {
//             GalVoice.PlayOneShot(zapperSound);
//             Animator.SetInteger(specialAnimationInt, 1);
//             playerToRecharge.inSpecialInteractAnimation = true;
//             yield return new WaitForSeconds(0.2f);
//             Animator.SetInteger(specialAnimationInt, -1);
//             while (playerToRecharge.currentlyHeldObjectServer.insertedBattery.charge < 1f)
//             {
//                 playerToRecharge.currentlyHeldObjectServer.insertedBattery.charge += Time.deltaTime;
//                 yield return null;
//             }
//             playerToRecharge.inSpecialInteractAnimation = false;
//         }
//         zapperRoutine = null;
//     }
//
//     private void OnKeyboardInteract(PlayerControllerB playerInteracting)
//     {
//         if (!playerInteracting.IsLocalPlayer || playerInteracting != ownerPlayer) return;
//         KeyboardInteractServerRpc();
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     private void KeyboardInteractServerRpc()
//     {
//         KeyboardInteractClientRpc();
//     }
//
//     [ClientRpc]
//     private void KeyboardInteractClientRpc()
//     {
//         StartCoroutine(terminalFaceController.TemporarySwitchEffect((int)Emotion.Flustered));
//         GalVoice.PlayOneShot(keyboardPressSound);
//         EnablePhysics(!physicsEnabled);
//     }
//     
//     public float DoCalculatePathDistance(NavMeshPath path)
//     {
//         float length = 0.0f;
//
//         if (path.status != NavMeshPathStatus.PathInvalid && path.corners.Length >= 1)
//         {
//             for (int i = 1; i < path.corners.Length; i++)
//             {
//                 length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
//             }
//         }
//         //Plugin.ExtendedLogging($"Path distance: {length}");
//         return length;
//     }
//
//     public override void ActivateGal(PlayerControllerB owner)
//     {
//         base.ActivateGal(owner);
//         ResetToChargerStation(State.Active, Emotion.Basis);
//         if (GalCharger is DiceCharger terminalCharger)
//         {
//             terminalCharger.animator.SetBool(DiceCharger.isOpenedAnimation, true);
//         }
//     }
//
//     private void ResetToChargerStation(State state, Emotion emotion)
//     {
//         if (!IsServer) return;
//         if (Agent.enabled) Agent.Warp(GalCharger.ChargeTransform.position);
//         transform.SetPositionAndRotation(GalCharger.ChargeTransform.position, GalCharger.ChargeTransform.rotation);
//         HandleStateAnimationSpeedChangesServerRpc((int)state, (int)emotion);
//     }
//
//     public override void DeactivateGal()
//     {
//         base.DeactivateGal();
//         ResetToChargerStation(State.Inactive, Emotion.Sleeping);
//         if (GalCharger is DiceCharger terminalCharger)
//         {
//             terminalCharger.animator.SetBool(DiceCharger.isOpenedAnimation, false);
//         }
//     }
//
//     private void InteractTriggersUpdate()
//     {
//         bool interactable = !inActive && (ownerPlayer != null && GameNetworkManager.Instance.localPlayerController == ownerPlayer);
//         bool idleInteractable = galState != State.UnlockingObjects && interactable;
//         keyboardInteractTrigger.interactable = interactable;
//         keyInteractTrigger.interactable = idleInteractable;
//         zapperInteractTrigger.interactable = idleInteractable;
//         teleporterInteractTrigger.interactable = idleInteractable;
//     }
//
//     private void StoppingDistanceUpdate()
//     {
//         Agent.stoppingDistance = 3f * (galState == State.UnlockingObjects ? 0.5f : 1f);
//     }
//
//     private void SetIdleDefaultStateForEveryone()
//     {
//         if (GalCharger == null || (IsServer && !doneOnce))
//         {
//             doneOnce = true;
//             // Plugin.Logger.LogInfo("Syncing for client");
//             galRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 69);
//             chargeCount = 0;
//             maxChargeCount = chargeCount;
//             Agent.enabled = false;
//             StartUpDelay();
//         }
//     }
//
//     public override void InActiveUpdate()
//     {
//         base.InActiveUpdate();
//         inActive = galState == State.Inactive;
//     }
//
//     public override void Update()
//     {
//         base.Update();
//         SetIdleDefaultStateForEveryone();
//         InteractTriggersUpdate();
//
//         if (inActive)
//         {
//             FootstepSource.volume = 0f;
//             return;
//         }
//         StoppingDistanceUpdate();
//         if (Animator.GetFloat(runSpeedFloat) > 0.01f)
//         {
//             FootstepSource.volume = 1f;
//         }
//         else
//         {
//             FootstepSource.volume = 0f;
//         }
//         if (galRandom.Next(500000) <= 3)
//         {
//             specialSource.Stop();
//             specialSource.Play();
//         }
//         if (!IsHost) return;
//         HostSideUpdate();
//     }
//
//     private float GetCurrentSpeedMultiplier()
//     {
//         return 1.3f;
//     }
//
//     private void HostSideUpdate()
//     {
//         if (StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.inShipPhase)
//         {
//             GalCharger.ActivateGirlServerRpc(-1);
//             return;
//         }
//         if (Agent.enabled) smartAgentNavigator.AdjustSpeedBasedOnDistance(GetCurrentSpeedMultiplier());
//         Animator.SetFloat(runSpeedFloat, Agent.velocity.magnitude / 2);
//         switch (galState)
//         {
//             case State.Inactive:
//                 break;
//             case State.Active:
//                 DoActive();
//                 break;
//             case State.FollowingPlayer:
//                 DoFollowingPlayer();
//                 break;
//             case State.Dancing:
//                 DoDancing();
//                 break;
//         }
//     }
//
//     public override void OnEnableOrDisableAgent(bool agentEnabled)
//     {
//         base.OnEnableOrDisableAgent(agentEnabled);
//         Animator.SetBool(flyingAnimation, !agentEnabled);
//     }
//
//     private void DoActive()
//     {
//         if (ownerPlayer == null)
//         {
//             GoToChargerAndDeactivate();
//             return;
//         }
//         else
//         {
//             HandleStateAnimationSpeedChanges(State.FollowingPlayer, Emotion.Basis);
//         }
//     }
//
//     private void DoFollowingPlayer()
//     {
//         if (ownerPlayer == null)
//         {
//             GoToChargerAndDeactivate();
//             return;
//         }
//
//         if (smartAgentNavigator.DoPathingToDestination(ownerPlayer.transform.position))
//         {
//             return;
//         }
//
//         DoStaringAtOwner(ownerPlayer);
//
//         if (DoDancingAction())
//         {
//             return;
//         }
//     }
//
//     private void DoDancing()
//     {
//     }
//     
//     private void PlayKeySoundAnimEvent()
//     {
//         GalVoice.PlayOneShot(keySound);
//     }
//
//     private void DoScrapActionsAnimEvent()
//     {
//        
//     }
//
//     private ParticleSystem DoTerrainScan()
//     {
//         //if (GameNetworkManager.Instance.localPlayerController != ownerPlayer || Vector3.Distance(transform.position, ownerPlayer.transform.position) > 10) return;
//         return terrainScanner.SpawnTerrainScanner(transform.position);
//     }
//
//     private bool DoDancingAction()
//     {
//         if (boomboxPlaying)
//         {
//             HandleStateAnimationSpeedChanges(State.Dancing, Emotion.VeryHappy);
//             StartCoroutine(StopDancingDelay());
//             return true;
//         }
//         return false;
//     }
//
//     private IEnumerator StopDancingDelay()
//     {
//         yield return new WaitUntil(() => !boomboxPlaying || galState != State.Dancing);
//         if (galState != State.Dancing) yield break;
//         HandleStateAnimationSpeedChanges(State.FollowingPlayer, Emotion.Basis);
//     }
//
//     private void StartFlyingAnimEvent()
//     {
//         SetFlying(true);
//         StartCoroutine(FlyAnimationDelay());
//     }
//
//     private IEnumerator FlyAnimationDelay()
//     {
//         smartAgentNavigator.cantMove = true;
//         yield return new WaitForSeconds(1.5f);
//         smartAgentNavigator.cantMove = false;
//     }
//
//     private void StopFlyingAnimEvent()
//     {
//         SetFlying(false);
//     }
//
//     private void SetFlying(bool Flying)
//     {
//         this.flying = Flying;
//         if (Flying)
//         {
//             GalSFX.PlayOneShot(startOrEndFlyingAudioClips[0]);
//             boostersGameObject.SetActive(true);
//             FlyingSource.volume = MysteryDice.SoundVolume.Value;
//         }
//         else
//         {
//             boostersGameObject.SetActive(false);
//             GalSFX.PlayOneShot(startOrEndFlyingAudioClips[1]);
//             FlyingSource.volume = 0f;
//         }
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     private void HandleStateAnimationSpeedChangesServerRpc(int state, int emotion)
//     {
//         HandleStateAnimationSpeedChanges((State)state, (Emotion)emotion);
//     }
//
//     private void HandleStateAnimationSpeedChanges(State state, Emotion emotion) // This is for host
//     {
//         SwitchStateClientRpc((int)state, (int)emotion);
//         switch (state)
//         {
//             case State.Inactive:
//                 SetAnimatorBools(dance: false, activated: false);
//                 break;
//             case State.Active:
//                 SetAnimatorBools(dance: false, activated: true);
//                 break;
//             case State.FollowingPlayer:
//                 SetAnimatorBools(dance: false, activated: true);
//                 break;
//             case State.Dancing:
//                 SetAnimatorBools(dance: true, activated: true);
//                 break;
//         }
//     }
//
//     private void SetAnimatorBools(bool dance, bool activated)
//     {
//         Animator.SetBool(danceAnimation, dance);
//         Animator.SetBool(activatedAnimation, activated);
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     private void SwitchStateServerRpc(int state, int emotion)
//     {
//         SwitchStateClientRpc(state, emotion);
//     }
//
//     [ClientRpc]
//     private void SwitchStateClientRpc(int state, int emotion)
//     {
//         SwitchState(state, emotion);
//     }
//
//     private void SwitchState(int state, int emotion) // this is for everyone.
//     {
//         State stateToSwitchTo = (State)state;
//         Emotion emotionToSwitchTo = (Emotion)emotion;
//
//         if (state != -1)
//         {
//             switch (stateToSwitchTo)
//             {
//                 case State.Inactive:
//                     HandleStateInactiveChange();
//                     break;
//                 case State.Active:
//                     HandleStateActiveChange();
//                     break;
//                 case State.FollowingPlayer:
//                     HandleStateFollowingPlayerChange();
//                     break;
//                 case State.Dancing:
//                     HandleStateDancingChange();
//                     break;
//                 case State.UnlockingObjects:
//                     HandleStateUnlockingObjectsChange();
//                     break;
//             }
//             galState = stateToSwitchTo;
//         }
//
//         //Plugin.ExtendedLogging($"Switching emotion to {emotionToSwitchTo}");
//         terminalFaceController.SetFaceState(emotionToSwitchTo, 100);
//     }
//
//     #region State Changes
//     private void HandleStateInactiveChange()
//     {
//         ownerPlayer = null;
//         Agent.enabled = false;
//         Animator.SetBool(flyingAnimation, false);
//     }
//
//     private void HandleStateActiveChange()
//     {
//         Agent.enabled = true;
//     }
//
//     private void HandleStateFollowingPlayerChange()
//     {
//         GalVoice.PlayOneShot(GreetOwnerSound);
//     }
//
//     private void HandleStateDancingChange()
//     {
//     }
//
//     private void HandleStateUnlockingObjectsChange()
//     {
//     }
//     #endregion
//
//     public override void OnUseEntranceTeleport(bool setOutside)
//     {
//         base.OnUseEntranceTeleport(setOutside);
//     }
//
//     public override void OnEnterOrExitElevator(bool enteredElevator)
//     {
//         base.OnEnterOrExitElevator(enteredElevator);
//     }
//
//     public IEnumerator DoCustomPassThing(ParticleSystem particleSystem, CustomPassManager.CustomPassType customPassType)
//     {
//         if (CustomPassManager.Instance.EnableCustomPass(customPassType, true) is not SeeThroughCustomPass customPass) yield break;
//
//         customPass.maxVisibilityDistance = 0f;
//
//         yield return new WaitWhile(() =>
//         {
//             float percentLifetime = particleSystem.time / particleSystem.main.startLifetime.constant;
//             customPass.maxVisibilityDistance = particleSystem.sizeOverLifetime.size.Evaluate(percentLifetime) * 300; // takes some odd seconds
//             return customPass.maxVisibilityDistance < Plugin.ModConfig.ConfigTerminalScanRange.Value;
//         });
//
//         yield return new WaitForSeconds(5);
//
//         yield return new WaitWhile(() =>
//         {
//             customPass.maxVisibilityDistance -= Time.deltaTime * Plugin.ModConfig.ConfigTerminalScanRange.Value / 3f; // takes 3s
//             return customPass.maxVisibilityDistance > 0f;
//         });
//         CustomPassManager.Instance.RemoveCustomPass(customPassType);
//     }
//
//     public override bool Hit(int force, Vector3 hitDirection, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
//     {
//         base.Hit(force, hitDirection, playerWhoHit, playHitSFX, hitID);
//         DoAngerGalServerRpc();
//         return true;
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     public void DoAngerGalServerRpc()
//     {
//         DoAngerGalClientRpc();
//     }
//
//     [ClientRpc]
//     private void DoAngerGalClientRpc()
//     {
//         if (terminalFaceController.TemporarySwitchCoroutine != null)
//         {
//             StopCoroutine(terminalFaceController.TemporarySwitchCoroutine);
//         }
//         terminalFaceController.TemporarySwitchCoroutine = StartCoroutine(terminalFaceController.TemporarySwitchEffect((int)Emotion.Angy));
//         terminalFaceController.glitchTimer = terminalFaceController.controllerRandom.NextFloat(4f, 8f);
//     }
// }
//
//
