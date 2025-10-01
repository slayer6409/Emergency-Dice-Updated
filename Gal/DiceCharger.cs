using System.Collections;
using Unity.Netcode;

using MysteryDice.Extensions;
using UnityEngine;

namespace MysteryDice.Gal;
public class DiceCharger : Charger
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        // Instantiate the DICE GAL prefab
        GalAI = Instantiate(MysteryDice.DiceGal, ChargeTransform.position, ChargeTransform.rotation).GetComponent<GalAI>();
        NetworkObject netObj = GalAI.GetComponent<NetworkObject>();
        GalAI.GalCharger = this;
        netObj.Spawn();
    }

}