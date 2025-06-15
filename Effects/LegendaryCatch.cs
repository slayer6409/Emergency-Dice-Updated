using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class LegendaryCatch : IEffect
    {
        public string Name => "Legendary Catch";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Go Catch a Legendary!";
        public void Use()
        {
            Networker.Instance.MasterballServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public static void SpawnMasterball(int playerID)
        {
            List<Item> items = UnityEngine.Resources.FindObjectsOfTypeAll<Item>().ToList();
            Item masterball = items.FirstOrDefault(item => item.name.Equals("Masterball"));
            GameObject obj = UnityEngine.Object.Instantiate(masterball.spawnPrefab,
                Misc.GetPlayerByUserID(playerID).transform.position,
                Quaternion.identity,
                RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();
            CullFactorySoftCompat.RefreshGrabbableObjectPosition(obj.GetComponent<GrabbableObject>());
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
        }
    }
}
