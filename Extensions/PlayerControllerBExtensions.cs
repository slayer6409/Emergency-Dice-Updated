using GameNetcodeStuff;

namespace MysteryDice.Extensions;

public static class PlayerControllerBExtensions
{
    public static bool IsLocalPlayer(this PlayerControllerB player)
    {
        return player == GameNetworkManager.Instance.localPlayerController;
    }
}