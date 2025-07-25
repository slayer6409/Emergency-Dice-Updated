using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using MysteryDice.Extensions;

namespace MysteryDice.Gal;
public class Charger : NetworkBehaviour
{
    public InteractTrigger ActivateOrDeactivateTrigger = null!;
    public Transform ChargeTransform = null!;
    [NonSerialized] public GalAI GalAI = null!;
    [NonSerialized] public static List<Charger> Instances = new();
    private static readonly int ActivatedAnimation = Animator.StringToHash("isActivated");

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instances.Add(this);
    }

    public IEnumerator ActivateGalAfterLand()
    {
        if (!IsServer) yield break;
        while (true)
        { 
            yield return new WaitUntil(() => TimeOfDay.Instance.normalizedTimeOfDay <= 0.12f && StartOfRound.Instance.shipHasLanded && !GalAI.Animator.GetBool(ActivatedAnimation) && !StartOfRound.Instance.shipIsLeaving && !StartOfRound.Instance.inShipPhase && RoundManager.Instance.currentLevel.levelID != 3);
            MysteryDice.ExtendedLogging("Activating  Gal" + TimeOfDay.Instance.normalizedTimeOfDay);
            if (!GalAI.Animator.GetBool(ActivatedAnimation))
            {
                PlayerControllerB closestPlayer = StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead).OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).First();
                int playerIndex = Array.IndexOf(StartOfRound.Instance.allPlayerScripts, closestPlayer);
                if (!NetworkObject.IsSpawned) yield break;
                ActivateGirlServerRpc(playerIndex);
            }
        }
    }

    public void OnActivateGal(PlayerControllerB playerInteracting)
    {
        MysteryDice.ExtendedLogging("Activate Ran");
        if (!NetworkObject.IsSpawned) return;
        if (playerInteracting == null || !playerInteracting.IsLocalPlayer()) return;
        if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipIsLeaving || (RoundManager.Instance.currentLevel.levelID == 3 && !MysteryDice.NavMeshInCompanyPresent)) return;
        if (!GalAI.Animator.GetBool(ActivatedAnimation))
        {
            ActivateGirlServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, playerInteracting));
        }
        else
        {
            if (MysteryDice.ConfigOnlyOwnerDisablesGal.Value && playerInteracting != GalAI.ownerPlayer) return;
            ActivateGirlServerRpc(-1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateGirlServerRpc(int index)
    {
        GalAI.Animator.SetBool(ActivatedAnimation, index != -1);
        ActivateGirlClientRpc(index);
    }

    [ClientRpc]
    private void ActivateGirlClientRpc(int index)
    {
        if (index != -1) GalAI.ActivateGal(StartOfRound.Instance.allPlayerScripts[index]);
        else GalAI.DeactivateGal();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Instances.Remove(this);
    }
}