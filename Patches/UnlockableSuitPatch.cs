using HarmonyLib;
using MysteryDice.Effects;

namespace MysteryDice.Patches;

public class UnlockableSuitPatch
{
    [HarmonyPatch(typeof(UnlockableSuit), nameof(UnlockableSuit.SwitchSuitForPlayer))]
    public class Patch_DisableSuitAudio
    {
        static void Prefix(ref bool playAudio)
        {
            if(!Confusion.isConfused) return;
            playAudio = false;
        }
    }
}