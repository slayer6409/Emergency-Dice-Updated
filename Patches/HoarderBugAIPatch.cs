using System;
using HarmonyLib;
using JetBrains.Annotations;
using MysteryDice.Dice;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Patches;

[HarmonyPatch(typeof(HoarderBugAI))]
public class HoarderBugAIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HoarderBugAI.Start))]
    public static void start(HoarderBugAI __instance)
    {
        HoarderBugCache hbc = __instance.gameObject.AddComponent<HoarderBugCache>();
        hbc.instance = __instance;
    }
}

public class HoarderBugCache : MonoBehaviour
{
    public float timeSince = 0;
    public HoarderBugAI instance;
    public GrabbableObject heldItem;
    public bool isDice = false;
    [CanBeNull] DieBehaviour die = null;
    private void Awake()
    {
        try
        {
            instance = GetComponent<HoarderBugAI>(); 
        }
        catch (Exception e)
        {
            
        }
    }
    public void Update()
    {
        try
        {
            if (instance.heldItem == null) return;
            if (instance.heldItem.itemGrabbableObject != heldItem)
            {
                die = null;
                isDice = false;
                heldItem = instance.heldItem.itemGrabbableObject;
                if (heldItem.TryGetComponent(out die))
                {
                    isDice = true;
                }
            }

            if (!StartOfRound.Instance.IsServer) return;
            if (!MysteryDice.yippeeUse.Value) return;
            if (isDice)
            {
                timeSince += Time.deltaTime;

                if (timeSince >= 5f)
                {
                    timeSince = 0f;
                    int rnum = UnityEngine.Random.Range(0, 100);
                    if (MysteryDice.aprilFoolsConfig.Value) rnum -= 10;
                    if (rnum <= 20)
                    {
                        die?.SyncDropServerRPC(6409046, UnityEngine.Random.Range(0, 10));
                        instance.heldItem = null;
                        isDice = false;
                    }
                }
            }
            else
            {
                timeSince = 0f; // Reset timer when not held
            }
        }
        catch
        {
            
        }
       
    }
}