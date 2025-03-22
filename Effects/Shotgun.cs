using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Shotgun : IEffect
    {
        public string Name => "Shotgun";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Spawning a shotgun!";
        public void Use()
        {
            Networker.Instance.ShotgunServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId);
        }

        public static void SpawnShotgun(ulong playerID)
        {
            try
            {
                List<Item> items = UnityEngine.Resources.FindObjectsOfTypeAll<Item>().ToList();
                Item shotgun = items.LastOrDefault(item => item.name.Equals("Shotgun") &&
                                            item.spawnPrefab != null &&
                                            item.spawnPrefab.GetComponents<Component>().Length == 10);
                Item ammo = items.LastOrDefault(item => item.name.Equals("GunAmmo") && 
                                            item.spawnPrefab != null &&
                                            item.spawnPrefab.GetComponents<Component>().Length == 7);
                

                if (shotgun == null)
                {
                    List<Item> shotguns = items.FindAll(item => item.name.Equals("Shotgun"));
                    int count = 0;
                    foreach (Item shtgn in shotguns)
                    {
                        count++;

                        List<Component> components = shtgn.spawnPrefab.GetComponents<Component>().ToList();
                        foreach (Component comp in components)
                        {
                            MysteryDice.CustomLogger.LogWarning("Shotgun "+count+": "+comp.GetType().Name);
                        }
                    }
                    //generally the normal shotgun is last for some reason (at least in my testing)
                    shotgun = items.Last(item => item.name.Equals("Shotgun"));
                }
                if (ammo == null)
                {
                    List<Item> ammos = items.FindAll(item => item.name.Equals("GunAmmo"));
                    int count = 0;
                    foreach (Item amo in ammos)
                    {
                        count++;

                        List<Component> components = amo.spawnPrefab.GetComponents<Component>().ToList();
                        foreach (Component comp in components)
                        {
                            MysteryDice.CustomLogger.LogWarning("Ammo " + count + ": " + comp.GetType().Name);
                        }
                    }
                    ammo = items.Last(item => item.name.Equals("GunAmmo"));
                }

                GameObject obj = UnityEngine.Object.Instantiate(shotgun.spawnPrefab,
                    Misc.GetPlayerByUserID(playerID).transform.position,
                    Quaternion.identity,
                    RoundManager.Instance.playersManager.propsContainer);

                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                var netob = obj.GetComponent<NetworkObject>();
                netob.Spawn();
                obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
                obj.GetComponent<GrabbableObject>().EnablePhysics(true);
                int ammoAmount = UnityEngine.Random.Range(2, 6);
                for (int i = 0; i < ammoAmount; i++)
                {

                    GameObject obj2 = UnityEngine.Object.Instantiate(ammo.spawnPrefab,
                        Misc.GetPlayerByUserID(playerID).transform.position,
                        Quaternion.identity,
                        RoundManager.Instance.playersManager.propsContainer);

                    obj2.GetComponent<GrabbableObject>().fallTime = 0f;
                    NetworkObject netob2 = obj2.GetComponent<NetworkObject>();
                    netob2.Spawn();
                    obj2.GetComponent<GrabbableObject>().EnableItemMeshes(true);
                    obj.GetComponent<GrabbableObject>().EnablePhysics(true);
                }
            }
            catch (Exception e) 
            {
                MysteryDice.CustomLogger.LogError(e);
            }
           
        }
    }
}
