using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class SilentTP : IEffect
    {
        public string Name => "Silent Teleporter Traps";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Teleporter Traps are invisible";

        public object TeleporterTrapScr;

        public void Use()
        {
            Networker.Instance.SilenceTPServerRPC();
        }

        public static IEnumerator SilenceAllTP(bool isServer)
        {
            if (isServer)
                TPTraps.SpawnTeleporterTraps(10);

            yield return new WaitForSeconds(5); //lazy fix to allow all clients to sync

            var landmines = GameObject.FindObjectsOfType<Component>().Where(c => c.GetType().Name == "TeleporterTrap");

            foreach (var mine in landmines)
            {
                Renderer[] renderers = mine.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    GameObject.Destroy(renderer);
                }
            }
        }
    }
}
