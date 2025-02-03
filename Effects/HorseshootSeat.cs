using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class HorseshootSeat : IEffect
    {
        public string Name => "Horseshoot Seat";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Horseshoot Seat";
        public void Use()
        {
            Networker.Instance.HorseSeatServerRpc();
        }

        public static void HorseSeat()
        {
            VehicleController car = GameObject.FindObjectsOfType<VehicleController>().FirstOrDefault();
            
            if (car != null)
            {
                var seat = car.gameObject.transform.Find("Triggers").Find("DriverSide").Find("DriverSeatPositionNode");
                GameObject gameObject = UnityEngine.Object.Instantiate(
                    GetEnemies.Horse.enemyType.enemyPrefab,
                    seat.transform.position-new Vector3(0, 0.2f,0),
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);

                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
                var netobj = gameObject.GetComponent<NetworkObject>();
                netobj.Spawn(destroyWithScene: true);
            }
        }
    }
    
}
