using GameNetcodeStuff;
using LethalLib.Modules;
using Mimics.API;
using Mimics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using DunGen;
using Object = UnityEngine.Object;
using Random = System.Random;
using System.Runtime.CompilerServices;

namespace MysteryDice.Effects
{
    internal class MimicDoors : IEffect
    {
        public string Name => "Mimics";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "More fire exits!";

        public void Use()
        {
            Networker.Instance.MimicsServerRPC();
        }

        public static void MakeMimic()
        {

            List<Doorway> list = new List<Doorway>();
            DunGen.Dungeon currentDungeon = RoundManager.Instance.dungeonGenerator.Generator.CurrentDungeon;
            foreach (Tile allTile in currentDungeon.AllTiles)
            {
                foreach (Doorway unusedDoorway in allTile.UnusedDoorways)
                {
                    if (unusedDoorway.HasDoorPrefabInstance || (Object)(object)((Component)unusedDoorway).GetComponentInChildren<SpawnSyncedObject>(true) == (Object)null)
                        {
                            continue;
                        }
                        var component = unusedDoorway.GetComponentInChildren<SpawnSyncedObject>(true);
                        if (component == null) { Debug.LogWarning("SpawnSyncedObject component not found"); return; }
                        GameObject gameObject = ((Component)((Component)((Component)unusedDoorway).GetComponentInChildren<SpawnSyncedObject>(true)).transform.parent).gameObject;
                        if (!((Object)gameObject).name.StartsWith("AlleyExitDoorContainer") || gameObject.activeSelf)
                        {
                            continue;
                        }
                        bool flag = false;
                        Matrix4x4 val = Matrix4x4.TRS(
                        ((Component)unusedDoorway).transform.position,
                        ((Component)unusedDoorway).transform.rotation,
                        new Vector3(1f, 1f, 1f)
                        );
                        Bounds val2 = new Bounds(
                            new Vector3(0f, 1.5f, 5.5f), // Center
                            new Vector3(2f, 6f, 8f)      // Size
                        );
                        val2.center = val.MultiplyPoint3x4(val2.center);
                        Collider[] array4 = Physics.OverlapBox(
                            val2.center,
                            val2.extents,
                            ((Component)unusedDoorway).transform.rotation,
                            LayerMask.GetMask("Room", "Railing", "MapHazards")
                            ); Collider[] array5 = array4;
                        int num8 = 0;
                        if (num8 < array5.Length)
                        {
                            Collider val3 = array5[num8];
                            flag = true;
                        }
                        if (flag)
                        {
                            continue;
                        }
                        foreach (Tile allTile2 in currentDungeon.AllTiles)
                        {
                            if (!((Object)(object)allTile == (Object)(object)allTile2))
                            {
                                Vector3 origin = ((Component)unusedDoorway).transform.position + 5f * ((Component)unusedDoorway).transform.forward;
                                Bounds val4 = UnityUtil.CalculateProxyBounds(((Component)allTile2).gameObject, ignoreSpriteRendererBounds: true, Vector3.up);

                                // Step 2: Create and configure the upward ray
                                Ray val5 = new Ray
                                {
                                    origin = origin,
                                    direction = Vector3.up
                                };

                                // Step 3: Check if the bounds intersect with the upward ray
                                if (val4.IntersectRay(val5) &&
                                    (((Object)allTile2).name.Contains("Catwalk") ||
                                     ((Object)allTile2).name.Contains("LargeForkTile") ||
                                     ((Object)allTile2).name.Contains("4x4BigStair") ||
                                     ((Object)allTile2).name.Contains("ElevatorConnector") ||
                                     (((Object)allTile2).name.Contains("StartRoom") && !((Object)allTile2).name.Contains("Manor"))))
                                {
                                    flag = true;
                                }

                                // Step 4: Configure the downward ray
                                val5.origin = origin;
                                val5.direction = Vector3.down;

                                // Step 5: Check if the bounds intersect with the downward ray
                                if (val4.IntersectRay(val5) &&
                                    (((Object)allTile2).name.Contains("MediumRoomHallway1B") ||
                                     ((Object)allTile2).name.Contains("LargeForkTile") ||
                                     ((Object)allTile2).name.Contains("4x4BigStair") ||
                                     ((Object)allTile2).name.Contains("ElevatorConnector") ||
                                     ((Object)allTile2).name.Contains("StartRoom")))
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            continue;
                        }
                        list.Add(unusedDoorway);
                    }
            }
            foreach (Doorway item in list)
            {
                GameObject gameObject2 = ((Component)((Component)((Component)item).GetComponentInChildren<SpawnSyncedObject>(true)).transform.parent).gameObject;
                GameObject val7 = UnityEngine.Object.Instantiate<GameObject>(Mimics.Mimics.MimicPrefab, ((Component)item).transform);
                val7.transform.position = gameObject2.transform.position;
                MimicDoor component = val7.GetComponent<MimicDoor>();
                component.scanNode.creatureScanID = Mimics.Mimics.MimicCreatureID;
                AudioSource[] componentsInChildren = val7.GetComponentsInChildren<AudioSource>(true);
                foreach (AudioSource val8 in componentsInChildren)
                {
                    val8.volume = Mimics.Mimics.MimicVolume / 100f;
                    val8.outputAudioMixerGroup = StartOfRound.Instance.ship3DAudio.outputAudioMixerGroup;
                }
                if (Mimics.Mimics.SpawnRates[5] == 9753)
                {
                    val7.transform.position = new Vector3(-7f, 0f, -10f);
                }
                MimicDoor.allMimics.Add(component);
                component.mimicIndex = UnityEngine.Random.Range(103, 6010313); //If this generates multiple of the same numbers you got unlucky
                GameObject gameObject3 = ((Component)((Component)item).transform.GetChild(0)).gameObject;
                gameObject3.SetActive(false);
                Bounds bounds = ((Collider)component.frameBox).bounds;
                Vector3 center = bounds.center;
                bounds = ((Collider)component.frameBox).bounds;
                Collider[] array6 = Physics.OverlapBox(center, bounds.extents, Quaternion.identity);
                foreach (Collider val9 in array6)
                {
                    if (((Object)((Component)val9).gameObject).name.Contains("Shelf"))
                    {
                        ((Component)val9).gameObject.SetActive(false);
                    }
                }
                Light componentInChildren = gameObject2.GetComponentInChildren<Light>(true);
                ((Component)componentInChildren).transform.parent.SetParent(val7.transform);
                MeshRenderer[] componentsInChildren2 = val7.GetComponentsInChildren<MeshRenderer>();
                MeshRenderer[] array7 = componentsInChildren2;
                foreach (MeshRenderer val10 in array7)
                {
                    Material[] materials = ((Renderer)val10).materials;
                    foreach (Material val11 in materials)
                    {
                        val11.shader = ((Renderer)gameObject3.GetComponentInChildren<MeshRenderer>(true)).material.shader;
                        val11.renderQueue = ((Renderer)gameObject3.GetComponentInChildren<MeshRenderer>(true)).material.renderQueue;
                    }
                }
                
                if (Mimics.Mimics.MimicPerfection)
                {
                    return;
                } 
                if (!Mimics.Mimics.ColorBlindMode)
                {
                    if ((StartOfRound.Instance.randomMapSeed) % 2 == 0)
                    {
                        ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[0]).material.color = new Color(0.490566f, 0.1226415f, 0.1302275f);
                        ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[1]).material.color = new Color(0.4339623f, 0.1043965f, 0.1150277f);
                        componentInChildren.colorTemperature = 1250f;
                    }
                    else
                    {
                        ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[0]).material.color = new Color(0.5f, 0.1580188f, 0.1657038f);
                        ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[1]).material.color = new Color(43f / 106f, 0.1358579f, 0.1393619f);
                        componentInChildren.colorTemperature = 1300f;
                    }
                }
                else if ((StartOfRound.Instance.randomMapSeed) % 2 == 0)
                {
                    component.interactTrigger.timeToHold = 1.1f;
                }
                else
                {
                    component.interactTrigger.timeToHold = 1f;
                }
                if (!Mimics.Mimics.EasyMode)
                {
                    

                    return;
                }
                Random random2 = new Random(StartOfRound.Instance.randomMapSeed);
                switch (random2.Next(0, 4))
                {
                    case 0:
                        if (!Mimics.Mimics.ColorBlindMode)
                        {
                            ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[0]).material.color = new Color(0.489f, 0.2415526f, 0.1479868f);
                            ((Renderer)val7.GetComponentsInChildren<MeshRenderer>()[1]).material.color = new Color(0.489f, 0.2415526f, 0.1479868f);
                        }
                        else
                        {
                            component.interactTrigger.timeToHold = 1.5f;
                        }
                        break;
                    case 1:
                        component.interactTrigger.hoverTip = "Feed : [LMB]";
                        component.interactTrigger.holdTip = "Feed : [LMB]";
                        break;
                    case 2:
                        component.interactTrigger.hoverIcon = component.LostFingersIcon;
                        break;
                    case 3:
                        component.interactTrigger.holdTip = "DIE : [LMB]";
                        component.interactTrigger.timeToHold = 0.5f;
                        break;
                    default:
                        component.interactTrigger.hoverTip = "BUG, REPORT TO DEVELOPER";
                        break;
                }
            }
                

        }
    }
}
