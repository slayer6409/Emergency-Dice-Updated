using MoreCompany;
using MoreCompany.Cosmetics;
using UnityEngine;

namespace MysteryDice.Effects;

public class WhereDidMyFriendsGoPt2 
{
    public static void ToggleCosmetics(int playerID, bool value)
    {
        var player = Misc.GetPlayerByUserID(playerID);
        CosmeticApplication cosmeticApplication = player.gameObject.GetComponentInChildren<CosmeticApplication>();
        if (!cosmeticApplication) return;
        cosmeticApplication.UpdateAllCosmeticVisibilities(false);

        foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
        {
            cosmetic.gameObject.SetActive(value);
        }
    }
}