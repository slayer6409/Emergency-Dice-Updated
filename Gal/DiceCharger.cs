// using Unity.Netcode;
//
// namespace MysteryDice.Gal;
//
// public class DiceCharger : Charger
// {
//     public override void OnNetworkSpawn()
//     {
//         base.OnNetworkSpawn();
//         if (!IsServer) return;
//         GalAI = Instantiate(MysteryDice.DiceGal, ChargeTransform.position, ChargeTransform.rotation).GetComponent<DiceGalAI>();
//         NetworkObject netObj = GalAI.GetComponent<NetworkObject>();
//         GalAI.GalCharger = this;
//         netObj.Spawn();
//         GalAI.gameObject.transform.parent = this.transform;
//     }
// }