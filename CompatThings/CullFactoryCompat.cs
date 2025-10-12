using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using UnityEngine;

namespace MysteryDice;

internal static class CullFactorySoftCompat
{
    private static readonly bool CullFactoryAvailable;
    private static readonly MethodInfo RefreshGrabbableMethod;
    private static readonly MethodInfo RefreshLightMethod;

    static CullFactorySoftCompat()
    {
        if (Chainloader.PluginInfos.TryGetValue("com.fumiko.CullFactory", out var info) &&
            info.Metadata.Version >= new Version(1, 5, 0))
        {
            var apiType = Type.GetType("CullFactory.Behaviours.API.DynamicObjectsAPI, CullFactory");
            if (apiType != null)
            {
                RefreshGrabbableMethod = apiType.GetMethod("RefreshGrabbableObjectPosition", BindingFlags.Public | BindingFlags.Static);
                RefreshLightMethod = apiType.GetMethod("RefreshLightPosition", BindingFlags.Public | BindingFlags.Static);
                CullFactoryAvailable = RefreshGrabbableMethod != null && RefreshLightMethod != null;
            }
        }
    }

    
    [MethodImpl(MethodImplOptions.NoInlining|MethodImplOptions.NoOptimization)]
    internal static void RefreshGrabbableObjectPosition(GrabbableObject item)
    {
        item.EnableItemMeshes(true); // base fallback
        if (CullFactoryAvailable)
        {
            RefreshGrabbableMethod?.Invoke(null, new object[] { item });
        }
    }

    
    [MethodImpl(MethodImplOptions.NoInlining|MethodImplOptions.NoOptimization)]
    internal static void RefreshLightPosition(Light light)
    {
        if (CullFactoryAvailable)
        {
            RefreshLightMethod?.Invoke(null, new object[] { light });
        }
    }
    
}