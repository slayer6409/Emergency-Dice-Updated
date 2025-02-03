// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using GameNetcodeStuff;
// using MysteryDice.Dice;
// using Unity.Netcode;
// using Unity.Netcode.Components;
// using UnityEngine;
// namespace MysteryDice.Gal;
//
// public class DiceGalAI : GalAI
// {
// public List<AudioClip> idleSounds = new();
// public List<AudioClip> diceEffectsSounds = new(); // Sounds for dice effects
// public List<int> effectChoices = new(); // List of possible effect outcomes
// public List<string> diceEffectsDescriptions = new(); // Descriptions of effects
//
// public InteractTrigger spinTrigger = null!;
// public InteractTrigger dealTrigger = null!;
// public InteractTrigger serveTrigger = null!;
// public InteractTrigger collisionTrigger = null!;
//
// private bool isDancing = false;
// private bool isRollingDice = false;
// private float idleAnimationTimer = 0f;
// private float idleSwitchInterval = 10f; // Switch every 10 seconds
//
// private bool physicsTemporarilyDisabled = false;
// private float hazardRevealTimer = 10f;
// private bool huggingOwner = false;
// private bool flying = false;
//
// private readonly static int rollDiceAnimation = Animator.StringToHash("rollDice"); // Trigger
// private readonly static int idleAnimation = Animator.StringToHash("idle"); // Integer
// private readonly static int effectChoiceAnimation = Animator.StringToHash("effectChoice"); // Integer
// private readonly static int activatedAnimation = Animator.StringToHash("activated"); // Bool
// private readonly static int dancingAnimation = Animator.StringToHash("dancing"); // Bool
// private readonly static int spinAnimation = Animator.StringToHash("spin"); // Trigger
// private readonly static int dealAnimation = Animator.StringToHash("deal"); // Trigger
// private readonly static int serveAnimation = Animator.StringToHash("serve"); // Trigger
//
// public override void OnNetworkSpawn()
// {
//     base.OnNetworkSpawn();
//
//     if (!IsServer) return;
//
//     spinTrigger.onInteract.AddListener(OnSpinInteract);
//     dealTrigger.onInteract.AddListener(OnDealInteract);
//     serveTrigger.onInteract.AddListener(OnServeInteract);
//     collisionTrigger.onInteract.AddListener(OnCollisionInteract);
//
//     StartUpDelay();
// }
//
// private void StartUpDelay()
// {
//     List<DiceCharger> diceChargers = new();
//     foreach (var charger in Charger.Instances)
//     {
//         if (charger is DiceCharger diceCharger)
//         {
//             diceChargers.Add(diceCharger);
//         }
//     }
//
//     if (diceChargers.Count <= 0)
//     {
//         if (IsServer) NetworkObject.Despawn();
//         Debug.LogError("DiceCharger not found in scene. DiceGalAI will not be functional.");
//         return;
//     }
//
//     DiceCharger nearestCharger = diceChargers
//         .OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
//
//     nearestCharger.GalAI = this;
// }
//
// public override void ActivateGal(PlayerControllerB owner)
// {
//     base.ActivateGal(owner);
//     Animator.SetBool(activatedAnimation, true);
//     ResetIdleState();
// }
//
// public override void DeactivateGal()
// {
//     base.DeactivateGal();
//     Animator.SetBool(activatedAnimation, false);
//     ResetIdleState();
// }
//
// public override void Update()
// {
//     base.Update();
//
//     if (isDancing || isRollingDice) return; // Skip idle updates while dancing or rolling dice
//
//     HandleIdleAnimation();
//     HandleTriggersUpdate();
// }
//
// private void HandleIdleAnimation()
// {
//     idleAnimationTimer += Time.deltaTime;
//     if (idleAnimationTimer >= idleSwitchInterval)
//     {
//         idleAnimationTimer = 0f;
//         int randomIdle = UnityEngine.Random.Range(1, 4); // Randomly choose between 1, 2, 3
//         Animator.SetInteger(idleAnimation, randomIdle);
//         StartCoroutine(ResetIdleAfterDelay());
//     }
// }
//
// private IEnumerator ResetIdleAfterDelay()
// {
//     yield return new WaitForSeconds(5f); // Keep the random idle for 5 seconds
//     ResetIdleState();
// }
//
// private void ResetIdleState()
// {
//     Animator.SetInteger(idleAnimation, 0); // Set back to the main idle animation
// }
//
// private void HandleTriggersUpdate()
// {
//     bool interactable = ownerPlayer != null;
//     spinTrigger.interactable = interactable;
//     dealTrigger.interactable = interactable;
//     serveTrigger.interactable = interactable;
// }
//
// private void OnSpinInteract(PlayerControllerB player)
// {
//     if (player != ownerPlayer) return;
//     Animator.SetTrigger(spinAnimation);
//     // GalSFX.PlayOneShot(spinSound);
//     StartCoroutine(HandleEffectChoice());
// }
//
// private IEnumerator HandleEffectChoice()
// {
//     yield return new WaitForSeconds(1f); // Wait for spin animation
//
//     int chosenEffect = UnityEngine.Random.Range(0, effectChoices.Count);
//     Animator.SetInteger(effectChoiceAnimation, chosenEffect);
//
//     if (diceEffectsSounds.Count > chosenEffect)
//     {
//         GalSFX.PlayOneShot(diceEffectsSounds[chosenEffect]);
//     }
//
//     Debug.Log($"DiceGalAI rolled an effect: {diceEffectsDescriptions[chosenEffect]}");
//
//     huggingOwner = chosenEffect == 1; // Example: setting a state based on effect
//     physicsTemporarilyDisabled = chosenEffect == 2; // Example: disabling physics
// }
//
// private void OnDealInteract(PlayerControllerB player)
// {
//     if (player != ownerPlayer) return;
//     Animator.SetTrigger(dealAnimation);
//     // GalSFX.PlayOneShot(dealSound);
//
//     // Add custom logic for "deal" interaction here
// }
//
// private void OnServeInteract(PlayerControllerB player)
// {
//     if (player != ownerPlayer) return;
//     Animator.SetTrigger(serveAnimation);
//     // GalSFX.PlayOneShot(serveSound);
//
//     // Add custom logic for "serve" interaction here
// }
//
// private void OnCollisionInteract(PlayerControllerB player)
// {
//     if (player != ownerPlayer) return;
//
//     Debug.Log("Collision interaction detected.");
// }
//
// public override void InActiveUpdate()
// {
//     base.InActiveUpdate();
//     if (boomboxPlaying && !isDancing)
//     {
//         Animator.SetBool(dancingAnimation, true);
//         isDancing = true;
//     }
//     else if (!boomboxPlaying && isDancing)
//     {
//         Animator.SetBool(dancingAnimation, false);
//         isDancing = false;
//     }
// }
//
// }
//
// // public InteractTrigger spinTrigger = null!;
// //     public InteractTrigger dealTrigger = null!;
// //     public InteractTrigger serveTrigger = null!;
// //     public InteractTrigger collisionTrigger = null!;
// //     public List<AudioClip> idleSounds = new();
// //     public AudioClip spinSound = null!;
// //     public AudioClip dealSound = null!;
// //     public AudioClip serveSound = null!;
// //     public List<AudioClip> diceEffectsSounds = new(); // Sounds for dice effects
// //     public List<int> effectChoices = new(); // List of possible effect outcomes
// //     public List<string> diceEffectsDescriptions = new(); // Descriptions of effects
// //     
// //     
// //     private bool isDancing = false;
// //     private bool isRollingDice = false;
// //     private float idleAnimationTimer = 0f;
// //     private float idleSwitchInterval = 10f; // Switch every 10 seconds
// //     
// //     private readonly static int rollDiceAnimation = Animator.StringToHash("rollDice"); // Trigger
// //     private readonly static int activatedAnimation = Animator.StringToHash("activated"); // Bool
// //     private readonly static int idleAnimation = Animator.StringToHash("idle"); // Integer
// //     private readonly static int effectChoiceAnimation = Animator.StringToHash("effectChoice"); // Integer
// //     
// //     public override void OnNetworkSpawn()
// //     {
// //         base.OnNetworkSpawn();
// //     
// //         if (!IsServer) return;
// //     
// //         spinTrigger.onInteract.AddListener(OnSpinInteract);
// //         dealTrigger.onInteract.AddListener(OnDealInteract);
// //         serveTrigger.onInteract.AddListener(OnServeInteract);
// //         collisionTrigger.onInteract.AddListener(OnCollisionInteract);
// //     }
// //     
// //     public override void ActivateGal(PlayerControllerB owner)
// //     {
// //         base.ActivateGal(owner);
// //         Animator.SetBool(activatedAnimation, true);
// //         ResetIdleState();
// //     }
// //     
// //     public override void DeactivateGal()
// //     {
// //         base.DeactivateGal();
// //         Animator.SetBool(activatedAnimation, false);
// //         ResetIdleState();
// //     }
// //     
// //     public override void Update()
// //     {
// //         base.Update();
// //     
// //         if (isDancing || isRollingDice) return; // Skip idle updates while dancing or rolling dice
// //     
// //         HandleIdleAnimation();
// //     }
// //     
// //     private void HandleIdleAnimation()
// //     {
// //         idleAnimationTimer += Time.deltaTime;
// //         if (idleAnimationTimer >= idleSwitchInterval)
// //         {
// //             idleAnimationTimer = 0f;
// //             int randomIdle = UnityEngine.Random.Range(1, 4); // Randomly choose between 1, 2, 3
// //             Animator.SetInteger(idleAnimation, randomIdle);
// //             StartCoroutine(ResetIdleAfterDelay());
// //         }
// //     }
// //     
// //     private IEnumerator ResetIdleAfterDelay()
// //     {
// //         yield return new WaitForSeconds(5f); // Keep the random idle for 5 seconds
// //         ResetIdleState();
// //     }
// //     
// //     private void ResetIdleState()
// //     {
// //         Animator.SetInteger(idleAnimation, 0); // Set back to the main idle animation
// //     }
// //     
// //     private void OnSpinInteract(PlayerControllerB player)
// //     {
// //         if (player != ownerPlayer) return;
// //         Animator.SetTrigger("spin");
// //         GalSFX.PlayOneShot(spinSound);
// //         StartCoroutine(HandleEffectChoice());
// //     }
// //     
// //     private IEnumerator HandleEffectChoice()
// //     {
// //         yield return new WaitForSeconds(1f); // Wait for spin animation
// //     
// //         int chosenEffect = UnityEngine.Random.Range(0, effectChoices.Count);
// //         Animator.SetInteger(effectChoiceAnimation, chosenEffect);
// //     
// //         if (diceEffectsSounds.Count > chosenEffect)
// //         {
// //             GalSFX.PlayOneShot(diceEffectsSounds[chosenEffect]);
// //         }
// //     
// //         Debug.Log($"DiceGalAI rolled an effect: {diceEffectsDescriptions[chosenEffect]}");
// //     
// //         // Handle the effect logic here (e.g., apply buffs, spawn items, etc.)
// //     
// //         isRollingDice = false;
// //     }
// //     
// //     private void OnDealInteract(PlayerControllerB player)
// //     {
// //         if (player != ownerPlayer) return;
// //         Animator.SetTrigger("deal");
// //         GalSFX.PlayOneShot(dealSound);
// //     
// //         // Add custom logic for "deal" interaction here
// //     }
// //     
// //     private void OnCollisionInteract(PlayerControllerB player)
// //     {
// //         if (player != ownerPlayer) return;
// //         // Add custom logic for collision interaction here
// //     }
// //     
// //     private void OnServeInteract(PlayerControllerB player)
// //     {
// //         if (player != ownerPlayer) return;
// //         Animator.SetTrigger("serve");
// //         GalSFX.PlayOneShot(serveSound);
// //     
// //         // Add custom logic for "serve" interaction here
// //     }
// //     
// //     public override void InActiveUpdate()
// //     {
// //         base.InActiveUpdate();
// //         if (boomboxPlaying && !isDancing)
// //         {
// //             Animator.SetBool("dancing", true);
// //             isDancing = true;
// //         }
// //         else if (!boomboxPlaying && isDancing)
// //         {
// //             Animator.SetBool("dancing", false);
// //             isDancing = false;
// //         }
// //     }
//
