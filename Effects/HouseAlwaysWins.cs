using LCTarrotCard.Util;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static MysteryDice.Effects.ZombieToShip;

namespace MysteryDice.Effects
{
    internal class HouseAlwaysWins : IEffect
    {
        public string Name => "House Always Wins";
        public EffectType Outcome => EffectType.GalAwful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "We Always win";

        public void Use()
        {
            Networker.Instance.HouseWinServerRPC();
        }
        public static void Spawn()
        {
            int MaskedSpawn = UnityEngine.Random.Range(3, 6);
            if (GetEnemies.Masked == null)
                return;
            for (int i = 0; i < MaskedSpawn; i++)
            {
                Vector3 position = StartOfRound.Instance.middleOfShipNode.position;
                GameObject gameObject = UnityEngine.Object.Instantiate(GetEnemies.allEnemies.Find(x=>x.enemyName=="Baboon hawk").enemyPrefab, position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();
            }
        }
    }
}
