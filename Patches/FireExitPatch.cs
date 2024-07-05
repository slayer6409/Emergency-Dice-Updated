using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class FireExitPatch
    {
        public static bool AreFireExitsBlocked = false;
        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPrefix]
        public static bool BlockEntrance(EntranceTeleport __instance)
        {
            if (AreFireExitsBlocked && __instance.entranceId != 0)
            {
                Misc.SafeTipMessage("Locked", "Something locked the fire exit?");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
