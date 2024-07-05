using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class ZombieToShip : IEffect
    {
        public string Name => "Zombie return swap";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Returns you to the ship and places a zombie in your original position";
        public void Use()
        {
            Networker.Instance.ZombieToShipServerRPC(StartOfRound.Instance.localPlayerController.playerClientId);
        }

        public static void ZombieUseServer(ulong clientID)
        {
            //RoundManager RM = RoundManager.Instance;
            PlayerControllerB player = Misc.GetPlayerByUserID(clientID);
            Vector3 position = player.transform.position;
            Networker.Instance.TeleportToShipClientRPC(clientID);

            //NetworkObjectReference netObj = Misc.SpawnEnemyOnServer(position, 0f, GetEnemies.Masked);

            GameObject gameObject = UnityEngine.Object.Instantiate(GetEnemies.Masked.enemyType.enemyPrefab, position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            gameObject.AddComponent<ZombieSuitData>().ZombieSuitID = player.currentSuitID;
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();

            ZombieSetSuit(netObj, player.currentSuitID);
        }

        public static void ZombieSetSuit(NetworkObjectReference netObj, int suitID)
        {
            if (netObj.TryGet(out var networkObject))
            {
                MaskedPlayerEnemy zombie = networkObject.GetComponent<MaskedPlayerEnemy>();
                zombie.SetSuit(suitID);
                if (zombie.creatureAnimator)
                    zombie.creatureAnimator.StopPlayback();
            }
        }

        public static void ZombieSyncData(NetworkObjectReference netObj)
        {
            if (netObj.TryGet(out var networkObject))
            {
                if (networkObject.TryGetComponent<ZombieSuitData>(out var zombieData))
                {
                    Networker.Instance.SyncSuitIDClientRPC(netObj, zombieData.ZombieSuitID);
                }
            }
        }

        public class ZombieSuitData : MonoBehaviour
        {
            public int ZombieSuitID = 9999;
        }
    }
}
