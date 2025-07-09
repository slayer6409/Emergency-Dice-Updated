using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace MysteryDice.Effects
{
    internal class IFeelSafe : IEffect
    {
        public string Name => "I Feel Safe";

        private static List<Vector3> allPositions = new List<Vector3>();
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "I Feel Safe";
        public void Use()
        {
            Networker.Instance.IFeelSafeServerRPC();
        }

    
        [MethodImpl (MethodImplOptions.NoInlining)]
        public static void SpawnCratesOutside(float positionOffsetRadius = 5f)
        {
            var safe = CodeRebirth.src.Plugin.Mod.MapObjectRegistry().Where(x => x.ObjectName == "Normal Metal Crate")
                .First();
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

            int maxAttempts = 100;
            
            Vector3 position = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position;
            Vector3 vector = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, default, random, -1) + (Vector3.up * 2);

            Physics.Raycast(vector, Vector3.down, out RaycastHit hit, 100, StartOfRound.Instance.collidersAndRoomMaskAndDefault);

            if (hit.collider != null)
            {
                Vector3 spawnPoint = hit.point + hit.normal * -0.75f;
                GameObject spawnedCrate = GameObject.Instantiate(safe.GameObject, spawnPoint, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
                spawnedCrate.transform.up = hit.normal;
                spawnedCrate.GetComponent<NetworkObject>().Spawn();
                SceneManager.MoveGameObjectToScene(spawnedCrate, RoundManager.Instance.mapPropsContainer.scene);
                Networker.Instance.TeleportOrBringPlayerToPosServerRPC(spawnedCrate.transform.position, Array.IndexOf(StartOfRound.Instance.allPlayerScripts, Misc.GetRandomAlivePlayer()));
            }
        
            
        }
        
        public static float GetShortestDistanceSqr(Vector3 position, List<Vector3> positions)
        {
            float shortestLength = float.MaxValue;
            foreach (Vector3 pos in positions)
            {
                float distance = (position - pos).sqrMagnitude;
                if (distance < shortestLength)
                    shortestLength = distance;
            }
            return shortestLength;
        }
    }

}
