using MysteryDice.Patches;
using System;
using System.Linq;
using CodeRebirth.src.Content.Maps;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class FollowerFan : IEffect
    {
        public string Name => "Follower Fan";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Your new best friend!";

        public void Use()
        {
            Networker.Instance.DoFollowerFanServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,Misc.GetRandomAlivePlayer()));
        }

        public static void giveFriend(int playerID)
        {
            var player = Misc.GetPlayerByUserID(playerID);
            var fan = GameObject.Instantiate(GetEnemies.Fan.prefabToSpawn,
                player.transform.position - player.transform.forward * 3f, Quaternion.identity);
            fan.name = "FollowerFan"+Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player);
            var netObj = fan.GetComponent<NetworkObject>();
            fan.GetComponentInChildren<IndustrialFanBackCollider>().gameObject.SetActive(false);
            fan.GetComponentInChildren<IndustrialFanFrontCollider>().industrialFan.pushForce *= -1;
            netObj.Spawn();
            Networker.Instance.setSizeClientRPC(netObj.NetworkObjectId,new Vector3(0.4f,0.4f,0.4f));
            Networker.Instance.AddMovingTrapClientRPC(fan.name,true,Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
        }
        public static void fixFan(GameObject fan)
        {
            var backCollider = fan.GetComponentInChildren<IndustrialFanBackCollider>();
            if (backCollider != null)
                backCollider.gameObject.SetActive(false);

            var frontCollider = fan.GetComponentInChildren<IndustrialFanFrontCollider>();
            if (frontCollider != null)
                frontCollider.industrialFan.pushForce *= -1;
        }
    }
}
