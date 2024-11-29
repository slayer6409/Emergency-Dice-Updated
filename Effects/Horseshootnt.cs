using BepInEx.Configuration;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static LethalThings.DynamicBone.DynamicBoneColliderBase;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Horseshootnt : IEffect
    {
        public string Name => "Horseshootnt";

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "WEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";

        public void Use()
        {
            Networker.Instance.SuperFlingerServerRPC(Misc.GetRandomAlivePlayer().playerClientId);
        }

        public static void spawnFlinger(ulong playerId)
        {
            var player = Misc.GetPlayerByUserID(playerId);
            GameObject enemyObject = UnityEngine.Object.Instantiate(
                GetEnemies.Horse.enemyType.enemyPrefab,
                player.transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            Networker.Instance.setHorseStuffServerRPC(enemyObject.GetComponent<NetworkObject>().NetworkObjectId, 2);
        }

        public static void horseStuff(GameObject obj)
        {
            try
            {
                obj.GetComponent<Surfaced.HorseAI>().launchForceIndex = 0;
                var horseAI = obj.GetComponent<Surfaced.HorseAI>();
                horseAI.launchForcesPerLevel = new float[] { -75f, -125f, -200 }; 
                obj.GetComponents<SkinnedMeshRenderer>()[0].materials[1].SetColor("Green", Color.green);
            }
            catch
            {

            }

        }
       
    }
}
