using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeRebirthLib.Util.INetworkSerializables;
using GameNetcodeStuff;
using MysteryDice.Dice;
using MysteryDice.Effects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using MysteryDice.Extensions;
using Debug = UnityEngine.Debug;

namespace MysteryDice.Gal;
public class DiceGalAI : GalAI
{
    [Header("Interact Triggers")]
    [SerializeField]
    private InteractTrigger _collisionTrigger = null!;
    [SerializeField]
    private InteractTrigger _onTheHouseInteractTrigger = null!;
    [SerializeField]
    private InteractTrigger _feelingLuckyInteractTrigger = null!;
    [SerializeField]
    private InteractTrigger _devilishDealInteractTrigger = null!;

    [Header("Sounds")]
    [SerializeField]
    private AudioSource _specialSource = null!;
    [SerializeField]
    private AudioSource _flyingSource = null!;
    [SerializeField]
    private AudioClip _onTheHouseSound = null!;
    [SerializeField]
    private AudioClip _feelingLuckySound = null!;
    [SerializeField]
    private AudioClip _devilishDealSound = null!;
    [SerializeField]
    private AudioClip _collisionSwitchSound = null!;
    [SerializeField]
    private AudioClip[] _startOrEndFlyingAudioClips = [];

    private bool flying = false;
    private Coroutine? _assRoutine = null;
    private Coroutine? _plateRoutine = null;
    private Coroutine? _feelingLuckyRoutine = null;
    private Coroutine? _idleResetCoroutine = null;
    private State galState = State.Inactive;

    private static readonly int DanceAnimation = Animator.StringToHash("isDancing"); // Bool
    private static readonly int ActivatedAnimation = Animator.StringToHash("isActivated"); // Bool
    private static readonly int RunSpeedFloat = Animator.StringToHash("RunSpeedFloat"); // Float
    private static readonly int FlyingAnimation = Animator.StringToHash("isFlying"); // Bool
    private static readonly int EffectChoice = Animator.StringToHash("effectChoice");
    private static readonly int spin = Animator.StringToHash("spin");
    private static readonly int deal = Animator.StringToHash("deal");
    private static readonly int serve = Animator.StringToHash("serve");
    private static readonly int RandomIdle = Animator.StringToHash("RandomIdle");

    public enum State
    {
        Inactive = 0,
        Active = 1,
        FollowingPlayer = 2,
        Dancing = 3,
        SpecialAction = 4,
        Idle = 5,
    }

    private void StartUpDelay()
    {
        List<DiceCharger> diceChargers = new();
        foreach (var charger in Charger.Instances)
        {
            if (charger is DiceCharger actuallyADiceCharger)
            {
                diceChargers.Add(actuallyADiceCharger);
            }
        }
        if (diceChargers.Count <= 0)
        {
            if (IsServer) NetworkObject.Despawn();
            MysteryDice.CustomLogger.LogError($"DiceCharger not found in scene. TerminalGalAI will not be functional.");
            return;
        }
        DiceCharger diceCharger = diceChargers.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First(); ;
        diceCharger.GalAI = this;
        GalCharger = diceCharger;
        // Automatic activation if configured
        if (MysteryDice.ConfigGalAutomatic.Value)
        {
            StartCoroutine(GalCharger.ActivateGalAfterLand());
        }

        // Adding listener for interaction trigger
        GalCharger.ActivateOrDeactivateTrigger.onInteract.AddListener(GalCharger.OnActivateGal);
        _collisionTrigger.onInteract.AddListener(OnChestInteract);
        _onTheHouseInteractTrigger.onInteract.AddListener(OnPlateTrigger);
        _devilishDealInteractTrigger.onInteract.AddListener(OnAssTrigger);
        _feelingLuckyInteractTrigger.onInteract.AddListener(OnHeadTrigger);
    }

    #region Chest Trigger
    private void OnChestInteract(PlayerControllerB playerInteracting)
    {
        if (!playerInteracting.IsLocalPlayer() || playerInteracting != ownerPlayer)
            return;

        OnChestInteractServerRpc(playerInteracting);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnChestInteractServerRpc(PlayerControllerReference playerControllerReference)
    {
        OnChestInteractClientRpc(playerControllerReference);
    }

    [ClientRpc]
    private void OnChestInteractClientRpc(PlayerControllerReference playerControllerReference)
    {
        GalVoice.PlayOneShot(_collisionSwitchSound);
        EnablePhysics(!physicsEnabled);
    }
    #endregion

    #region Ass Trigger
    private void OnAssTrigger(PlayerControllerB playerInteracting)
    {
        if (_assRoutine != null || !playerInteracting.IsLocalPlayer() || playerInteracting != ownerPlayer)
            return;

        OnAssInteractServerRpc(playerInteracting);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnAssInteractServerRpc(PlayerControllerReference playerControllerReference)
    {
        HandleStateAnimationSpeedChanges(State.SpecialAction);
        OnAssInteractClientRpc(playerControllerReference);
        NetworkAnimator.SetTrigger(deal);
    }

    [ClientRpc]
    private void OnAssInteractClientRpc(PlayerControllerReference playerControllerReference)
    {
        _assRoutine = StartCoroutine(DoDevilishDeal(playerControllerReference));
    }

    private IEnumerator DoDevilishDeal(PlayerControllerB playerDoingDeal)
    {
        yield return null;
        if (IsServer && galState == State.SpecialAction)
        {
            HandleStateAnimationSpeedChangesServerRpc((int)State.FollowingPlayer);
        }
        _assRoutine = null;
    }
    #endregion

    #region Plate Interact
    private void OnPlateTrigger(PlayerControllerB playerInteracting)
    {
        if (_plateRoutine != null || !playerInteracting.IsLocalPlayer() || playerInteracting != ownerPlayer)
            return;

        OnPlateInteractServerRpc(playerInteracting);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnPlateInteractServerRpc(PlayerControllerReference playerControllerReference)
    {
        HandleStateAnimationSpeedChanges(State.SpecialAction);
        OnPlateInteractClientRpc(playerControllerReference);
        NetworkAnimator.SetTrigger(serve);
    }
    
    [ClientRpc]
    private void OnPlateInteractClientRpc(PlayerControllerReference playerControllerReference)
    {
        _plateRoutine = StartCoroutine(OnTheHouseDeal(playerControllerReference));
    }

    private IEnumerator OnTheHouseDeal(PlayerControllerB playerDoingDeal)
    {
        yield return null;
        if (IsServer && galState == State.SpecialAction)
        {
            HandleStateAnimationSpeedChangesServerRpc((int)State.FollowingPlayer);
        }
        _plateRoutine = null;
    }
    #endregion

    #region Head Interact
    private void OnHeadTrigger(PlayerControllerB playerInteracting)
    {
        if (_feelingLuckyRoutine != null || !playerInteracting.IsLocalPlayer() || playerInteracting != ownerPlayer) return;
        OnHeadInteractServerRpc(playerInteracting);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnHeadInteractServerRpc(PlayerControllerReference playerControllerReference)
    {
        HandleStateAnimationSpeedChanges(State.SpecialAction);
        OnHeadInteractClientRpc(playerControllerReference);
        NetworkAnimator.SetTrigger(spin);
    }

    [ClientRpc]
    private void OnHeadInteractClientRpc(PlayerControllerReference playerControllerReference)
    {
        _feelingLuckyRoutine = StartCoroutine(FeelingLuckyDeal(playerControllerReference));
    }

    private IEnumerator FeelingLuckyDeal(PlayerControllerB playerDoingDeal)
    {
        yield return null;
        if (IsServer && galState == State.SpecialAction)
        {
            HandleStateAnimationSpeedChangesServerRpc((int)State.FollowingPlayer);
            NetworkAnimator.Animator.SetFloat(EffectChoice, 0);
            var effectToUse = DieBehaviour.getRandomEffectByType(EffectType.SpecialPenalty, DieBehaviour.AllEffects);
            yield return new WaitForSeconds(3);
            switch (effectToUse.Outcome)
            {
                case EffectType.Awful:
                    Animator.SetInteger(EffectChoice, 1);
                    break;
                case EffectType.Bad:
                    Animator.SetInteger(EffectChoice, 2);
                    break;
                case EffectType.Mixed:
                    Animator.SetInteger(EffectChoice, 3);
                    break;
                case EffectType.Great:
                    Animator.SetInteger(EffectChoice, 4);
                    break;
                default:
                    Animator.SetInteger(EffectChoice, 2);
                    break;
            }

            yield return new WaitForSeconds(2);
            NetworkAnimator.Animator.SetFloat(EffectChoice, 0);
        }
        _feelingLuckyRoutine = null;
    }
    #endregion

    public float DoCalculatePathDistance(NavMeshPath path)
    {
        float length = 0.0f;

        if (path.status != NavMeshPathStatus.PathInvalid && path.corners.Length >= 1)
        {
            for (int i = 1; i < path.corners.Length; i++)
            {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }
        MysteryDice.ExtendedLogging($"Path distance: {length}");
        return length;
    }

    public override void ActivateGal(PlayerControllerB owner)
    {
        base.ActivateGal(owner);
        ResetToChargerStation(State.Active);
    }

    private void ResetToChargerStation(State state)
    {
        if (!IsServer)
            return;

        if (Agent.enabled)
            Agent.Warp(GalCharger.ChargeTransform.position);

        transform.SetPositionAndRotation(GalCharger.ChargeTransform.position, GalCharger.ChargeTransform.rotation);
        HandleStateAnimationSpeedChangesServerRpc((int)state);
    }

    public override void DeactivateGal()
    {
        base.DeactivateGal();
        ResetToChargerStation(State.Inactive);
    }

    private void InteractTriggersUpdate()
    {
        bool interactable = !inActive && (ownerPlayer != null && GameNetworkManager.Instance.localPlayerController == ownerPlayer);
        bool idleInteractable = galState != State.SpecialAction && interactable && galState != State.Idle;
        _onTheHouseInteractTrigger.interactable = interactable;
        _devilishDealInteractTrigger.interactable = idleInteractable;
        _feelingLuckyInteractTrigger.interactable = idleInteractable;
        _collisionTrigger.interactable = idleInteractable;
    }

    
    private void StoppingDistanceUpdate()
    {
        Agent.stoppingDistance = 3f;
    }

    private void SetIdleDefaultStateForEveryone()
    {
        if (GalCharger == null || (IsServer && !doneOnce))
        {
            doneOnce = true;
            MysteryDice.CustomLogger.LogInfo("Syncing for client");
            galRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 69);
            chargeCount = 0;
            maxChargeCount = chargeCount;
            Agent.enabled = false;
            StartUpDelay();
        }
    }

    public override void InActiveUpdate()
    {
        base.InActiveUpdate();
        inActive = galState == State.Inactive;
    }

    public override void Update()
    {
        base.Update();
        SetIdleDefaultStateForEveryone();
        InteractTriggersUpdate();

        if (inActive)
        {
            return;
        }
        StoppingDistanceUpdate();
        if (galRandom.Next(500000) <= 3)
        {
            _specialSource.Stop();
            _specialSource.Play();
        }
        if (!IsHost) return;
        HostSideUpdate();
    }

    private float GetCurrentSpeedMultiplier()
    {
        return 1.3f;
    }

    private void HostSideUpdate()
    {
        if (StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.inShipPhase)
        {
            GalCharger.ActivateGirlServerRpc(-1);
            return;
        }
        if (Agent.enabled)
            smartAgentNavigator.AdjustSpeedBasedOnDistance(GetCurrentSpeedMultiplier());

        if(galRandom.Next(50000)<=3 
           && Agent.velocity.magnitude<0.05f 
           && galState != State.SpecialAction 
           && galState != State.Inactive 
           && galState != State.Dancing 
           && _idleResetCoroutine==null) 
                HandleStateAnimationSpeedChanges(State.Idle);
        
        Animator.SetFloat(RunSpeedFloat, Agent.velocity.magnitude / 2);
        switch (galState)
        {
            case State.Inactive:
                break;
            case State.Active:
                DoActive();
                break;
            case State.FollowingPlayer:
                DoFollowingPlayer();
                break;
            case State.Dancing:
                DoDancing();
                break;
            case State.SpecialAction:
                DoSpecialActions();
                break;
            case State.Idle:
                DoRandomIdle();
                break;
        }
    }

    private void DoRandomIdle()
    {
        if(_idleResetCoroutine!=null) return;
        Animator.SetInteger(RandomIdle, galRandom.Next(1, 4));
        _idleResetCoroutine=StartCoroutine(ResetIdle());
    }

    public IEnumerator ResetIdle()
    {
        yield return null; 
        var clipInfo = Animator.GetCurrentAnimatorClipInfo(0);
        float waitTime = 1.0f;
        if (clipInfo.Length > 0)
        {
            waitTime = clipInfo[0].clip.length;
        }
        yield return new WaitForSeconds(waitTime);
        HandleStateAnimationSpeedChangesServerRpc((int)State.FollowingPlayer);
        Animator.SetInteger(RandomIdle, 0);
        _idleResetCoroutine = null;
    }


    public override void OnEnableOrDisableAgent(bool agentEnabled)
    {
        base.OnEnableOrDisableAgent(agentEnabled);
        Animator.SetBool(FlyingAnimation, !agentEnabled);
    }

    private void DoActive()
    {
        if (ownerPlayer == null)
        {
            GoToChargerAndDeactivate();
            return;
        }
        else
        {
            HandleStateAnimationSpeedChanges(State.FollowingPlayer);
        }
    }

    private void DoFollowingPlayer()
    {
        if (ownerPlayer == null)
        {
            GoToChargerAndDeactivate();
            return;
        }

        if (smartAgentNavigator.DoPathingToDestination(ownerPlayer.transform.position))
        {
            return;
        }

        DoStaringAtOwner(ownerPlayer);

        if (DoDancingAction())
        {
            return;
        }
    }

    private void DoDancing()
    {
    }

    private void DoSpecialActions()
    {
    }

    private void PlayDevilishDealSoundAnimEvent()
    {
        GalVoice.PlayOneShot(_devilishDealSound);
    }

    private bool DoDancingAction()
    {
        if (boomboxPlaying)
        {
            HandleStateAnimationSpeedChanges(State.Dancing);
            StartCoroutine(StopDancingDelay());
            return true;
        }
        return false;
    }

    private IEnumerator StopDancingDelay()
    {
        yield return new WaitUntil(() => !boomboxPlaying || galState != State.Dancing);
        if (galState != State.Dancing)
            yield break;

        HandleStateAnimationSpeedChanges(State.FollowingPlayer);
    }

    #region Animation Events
    private void StartFlyingAnimEvent()
    {
        SetFlying(true);
        StartCoroutine(FlyAnimationDelay());
    }

    private void StopFlyingAnimEvent()
    {
        SetFlying(false);
    }
    #endregion

    private IEnumerator FlyAnimationDelay()
    {
        smartAgentNavigator.cantMove = true;
        yield return new WaitForSeconds(1.5f);
        smartAgentNavigator.cantMove = false;
    }

    private void SetFlying(bool Flying)
    {
        this.flying = Flying;
        if (Flying)
        {
            GalSFX.PlayOneShot(_startOrEndFlyingAudioClips[0]);
            _flyingSource.volume = MysteryDice.SoundVolume.Value;
        }
        else
        {
            GalSFX.PlayOneShot(_startOrEndFlyingAudioClips[1]);
            _flyingSource.volume = 0f;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleStateAnimationSpeedChangesServerRpc(int state)
    {
        HandleStateAnimationSpeedChanges((State)state);
    }

    //Only Host
    //Use when I want to change state
    private void HandleStateAnimationSpeedChanges(State state) 
    {
        SwitchStateClientRpc((int)state);
        switch (state)
        {
            case State.Inactive:
                SetAnimatorBools(dance: false, activated: false);
                break;
            case State.Active:
                SetAnimatorBools(dance: false, activated: true);
                break;
            case State.FollowingPlayer:
                SetAnimatorBools(dance: false, activated: true);
                break;
            case State.Dancing:
                SetAnimatorBools(dance: true, activated: true);
                break;
            case State.SpecialAction:
                SetAnimatorBools(dance: false, activated: true);
                break;
            case State.Idle:
                SetAnimatorBools(dance: false, activated: true);
                break;
        }
    }

    private void SetAnimatorBools(bool dance, bool activated)
    {
        Animator.SetBool(DanceAnimation, dance);
        Animator.SetBool(ActivatedAnimation, activated);
    }

    [ClientRpc]
    private void SwitchStateClientRpc(int state)
    {
        SwitchState(state);
    }

    private void SwitchState(int state) // this is for everyone.
    {
        State stateToSwitchTo = (State)state;

        if (state != -1)
        {
            switch (stateToSwitchTo)
            {
                case State.Inactive:
                    HandleStateInactiveChange();
                    break;
                case State.Active:
                    HandleStateActiveChange();
                    break;
                case State.FollowingPlayer:
                    HandleStateFollowingPlayerChange();
                    break;
                case State.Dancing:
                    HandleStateDancingChange();
                    break;
                case State.SpecialAction:
                    HandleStateSpecialActionsChange();
                    break;
                case State.Idle:
                    HandleStateIdleChange();
                    break;
            }
            galState = stateToSwitchTo;
        }
    }

    #region State Changes
    private void HandleStateInactiveChange()
    {
        ownerPlayer = null;
        Agent.enabled = false;
        Animator.SetBool(FlyingAnimation, false);
    }

    private void HandleStateActiveChange()
    {
        Agent.enabled = true;
    }

    private void HandleStateFollowingPlayerChange()
    {
        GalVoice.PlayOneShot(GreetOwnerSound);
    }

    private void HandleStateDancingChange()
    {
    }

    private void HandleStateSpecialActionsChange()
    {
    } 
    private void HandleStateIdleChange()
    {
    }
    #endregion

    public override void OnUseEntranceTeleport(bool setOutside)
    {
        base.OnUseEntranceTeleport(setOutside);
    }

    public override void OnEnterOrExitElevator(bool enteredElevator)
    {
        base.OnEnterOrExitElevator(enteredElevator);
    }

    public override bool Hit(int force, Vector3 hitDirection, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        base.Hit(force, hitDirection, playerWhoHit, playHitSFX, hitID);
        return true;
    }
}
