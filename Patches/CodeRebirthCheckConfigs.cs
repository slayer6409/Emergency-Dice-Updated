using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Patches;
using CodeRebirth;

public class CodeRebirthCheckConfigs
{
    public static bool checkFanConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigIndustrialFanEnabled.Value;
    } 
    public static bool checkMicrowaveConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigFunctionalMicrowaveEnabled.Value;
    } 
    public static bool checkACUnitConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigAirControlUnitEnabled.Value;
    } 
    public static bool checkLaserTurretConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigLaserTurretEnabled.Value;
    } 
    public static bool checkFlashConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigFlashTurretEnabled.Value;
    }
    public static bool checkBearTrapConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigBearTrapEnabled.Value;
    }
    public static bool checkCrateConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigItemCrateEnabled.Value;
    }
    public static bool checkTransporterConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigTransporterEnabled.Value;
    }
    public static bool checkJanitorConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigJanitorEnabled.Value;
    }
    public static bool checkTornadoConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigTornadosEnabled.Value;
    }
    public static bool checkZortConfig()
    {
        return CodeRebirth.src.Plugin.ModConfig.ConfigZortAddonsEnabled.Value;
    }

    public static List<trap> getSpawnPrefabs()
    {
        List<trap> traps = new List<trap>();
        traps.Add(new trap("BoomTrap",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.BearTrap.BoomTrapPrefab));
        traps.Add(new trap("GrassBeartrap",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.BearTrap.GrassMatPrefab));
        traps.Add(new trap("SnowBeartrap",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.BearTrap.SnowMatPrefab));
        traps.Add(new trap("GravelBeartrap",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.BearTrap.GravelMatPrefab));
        traps.Add(new trap("Crate",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.Crate.WoodenCratePrefab));
        traps.Add(new trap("Safe",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.Crate.MetalCratePrefab));
        traps.Add(new trap("MimicCrate",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.Crate.MimicWoodenCratePrefab));
        traps.Add(new trap("MimicSafe",CodeRebirth.src.Content.Maps.MapObjectHandler.Instance.Crate.MimicMetalCratePrefab));
        return traps;
    }
    
    
    
    
}