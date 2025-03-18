using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using System.Reflection;
using CodeRebirth.src.Content.Maps;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MovingBeartraps : IEffect
    {
        public string Name => "Roomba Beartraps";

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Roomba Beartraps?";

        private static List<GameObject> bearTraps = new List<GameObject>();

        public void Use()
        {
            Networker.Instance.MovingBeartrapsServerRPC();
        }

        public static IEnumerator fixSpiney(GameObject obj)
        {
            yield return new WaitForSeconds(0.1f);
            var laser = obj.GetComponentInChildren<LaserTurret>();
            if (laser!=null)
            { 
                laser.rotationSpeed = 800;
            }
            else
            {
                laser = obj.GetComponent<LaserTurret>();
                if (laser != null)
                {
                    laser.rotationSpeed = 800;
                }
            }
        }
    }
}