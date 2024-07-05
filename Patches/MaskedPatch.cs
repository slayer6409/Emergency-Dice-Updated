using HarmonyLib;
using Unity.Netcode;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void MaskedOnStart(MaskedPlayerEnemy __instance)
        {
            if (__instance.IsServer) return;

            Networker.Instance.RequestToSyncSuitIDServerRPC(__instance.GetComponent<NetworkObject>());
        }
    }
}
