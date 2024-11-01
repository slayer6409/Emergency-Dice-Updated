using LCTarrotCard.Util;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using static MysteryDice.Effects.ZombieToShip;

namespace MysteryDice.Effects
{
    internal class SpicyNuggies : IEffect
    {
        public string Name => "Spicy Nuggies";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spicy Nuggies!!!";

        public void Use()
        {
            Networker.Instance.spicyNuggiesServerRPC();
        }
        public static void Spawn()
        {
            int MaskedSpawn = UnityEngine.Random.Range(2, 7);
            if (GetEnemies.Masked == null)
                return;
            for (int i = 0; i < MaskedSpawn; i++)
            {
                Vector3 position = StartOfRound.Instance.middleOfShipNode.position;
                GameObject gameObject = UnityEngine.Object.Instantiate(GetEnemies.Masked.enemyType.enemyPrefab, position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                gameObject.AddComponent<ZombieSuitData>().ZombieSuitID = Misc.GetPlayerByUserID(Misc.GetRandomPlayerID()).currentSuitID;
                gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();
            }
        }
    }
}
