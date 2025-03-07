using BepInEx.Configuration;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static LethalThings.DynamicBone.DynamicBoneColliderBase;
using static UnityEngine.EventSystems.EventTrigger;

namespace MysteryDice.Effects
{
    internal class Flinger : IEffect
    {
        public string Name => "Flinger";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Get Flung";

        public static ConfigEntry<bool> beybladeMode;
        public void Use()
        {
            if (GetEnemies.Horse == null)
                return;
            Networker.Instance.spawnFlingerServerRPC(StartOfRound.Instance.localPlayerController.actualClientId);
        }
        public static void spawnHorseshoe(ulong userID)
        {
            var player = Misc.GetPlayerByUserID(userID);
            GameObject enemyObject = UnityEngine.Object.Instantiate(
                           GetEnemies.Horse.enemyType.enemyPrefab,
                           player.transform.position,
                           Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            if(beybladeMode.Value)enemyObject.AddComponent<TopSpin>();
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            Networker.Instance.setHorseStuffServerRPC(enemyObject.GetComponent<NetworkObject>().NetworkObjectId);
        }
        public static void horseStuff(GameObject obj)
        {
            try
            {
                obj.GetComponent<Surfaced.HorseAI>().launchForceIndex = 2;
                obj.GetComponents<SkinnedMeshRenderer>()[0].materials[1].SetColor("red", Color.red);
            }
            catch
            {

            }

        }
        public static void Config()
        {
            beybladeMode = MysteryDice.BepInExConfig.Bind<bool>(
              "Flinger",
              "Beyblade",
              false,
              "Makes the flinger a Beyblade Flinger");
           

        }
        public class TopSpin : MonoBehaviour
        {
            public float spinSpeed = 10000f; 

            void Update()
            {
                transform.Rotate(Vector3.up * (spinSpeed * Time.deltaTime));
            }
        }
    }
}
