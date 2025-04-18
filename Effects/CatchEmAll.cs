using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class CatchEmAll : IEffect
    {
        public string Name => "Catch Em All!";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Go Catch Em All!";
        public void Use()
        {
            Networker.Instance.PokeballsServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public static void SpawnPokeballs(int playerID)
        {
            List<Item> items = UnityEngine.Resources.FindObjectsOfTypeAll<Item>().ToList();
            Item pokeball = items.FirstOrDefault(item => item.name.Equals("Pokeball"));
            Item greatball = items.FirstOrDefault(item => item.name.Equals("Greatball"));
            Item ultraball = items.FirstOrDefault(item => item.name.Equals("Ultraball"));
            List<Item> balls = new List<Item>();
            if (pokeball    != null){ balls.Add(pokeball);  balls.Add(pokeball);    balls.Add(pokeball); }
            if (greatball   != null){ balls.Add(greatball); balls.Add(greatball);   }
            if (ultraball   != null){ balls.Add(ultraball); }
            

            if (balls.Count == 0) return; // Exit if no balls were found


            int pokeballAmount = UnityEngine.Random.Range(2, 4);
            for (int i = 0; i < pokeballAmount; i++)
            {
                int e = UnityEngine.Random.Range(0,balls.Count);
                GameObject obj2 = UnityEngine.Object.Instantiate(balls[e].spawnPrefab,
                    Misc.GetPlayerByUserID(playerID).transform.position,
                    Quaternion.identity,
                    RoundManager.Instance.playersManager.propsContainer);

                obj2.GetComponent<GrabbableObject>().fallTime = 0f;
                obj2.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
