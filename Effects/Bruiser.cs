using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Bruiser : IEffect
    {
        public string Name => "Bruiser";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Bruce in Cruiser";
        public void Use()
        {
            Networker.Instance.BruiserServerRpc();
        }

        public static void BruceInCruiser()
        {
            VehicleController car = GameObject.FindObjectsOfType<VehicleController>().FirstOrDefault();
            if (car != null)
            { 
                GameObject gameObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Bruce.enemyType.enemyPrefab,
                    car.transform.position-new Vector3(0, 1f, 0),
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);

                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                var netobj = gameObject.GetComponent<NetworkObject>();
                netobj.Spawn(destroyWithScene: true);
                Networker.Instance.FixBruceClientRpc(netobj.NetworkObjectId);
                Networker.Instance.DespawnObjectTimedServerRpc(netobj.NetworkObjectId);
            }
        }

        public static IEnumerator despawnObjectTimed(ulong obj)
        {
            yield return new WaitForSeconds(15f);

            Networker.Instance.DespawnObjectClientRpc(obj);
        }
        // public static IEnumerator TeleportObjectTimed(ulong obj, bool inside)
        // {
        //     yield return new WaitForSeconds(15f);
        //
        //     Networker.Instance.TeleportObjectClientRpc(obj, inside);
        // }
    }
    
}
