using BepInEx.Configuration;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static LethalThings.DynamicBone.DynamicBoneColliderBase;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class SuperFlinger : IEffect
    {
        private static readonly int Black = Shader.PropertyToID("Black");
        public string Name => "Super Flinger";

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
            Networker.Instance.setHorseStuffServerRPC(enemyObject.GetComponent<NetworkObject>().NetworkObjectId, 1);
        }

        public static void horseStuff(GameObject obj, int rand1, int rand2, int rand3)
        {
            try
            {
                obj.GetComponent<Surfaced.HorseAI>().launchForceIndex = 2;
                var horseAI = obj.GetComponent<Surfaced.HorseAI>();
                horseAI.launchForcesPerLevel= new float[] { 20f, 30f, 100f };
                obj.GetComponents<SkinnedMeshRenderer>()[0].materials[1].SetColor("Red", Color.red);
            }
            catch
            {

            }

        }
       
    }
}
