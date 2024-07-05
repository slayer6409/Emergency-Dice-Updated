using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        public static bool FastCharging = false;
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool ForceFastShooting(Turret __instance, ref float ___turretInterval, TurretMode ___turretMode)
        {
            if(___turretMode == TurretMode.Charging && FastCharging)
                ___turretInterval = 100f;

            return true;
        }
    }
}
