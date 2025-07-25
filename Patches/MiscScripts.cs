using Unity.Netcode.Components;
using UnityEngine;

namespace MysteryDice.Patches;
[DisallowMultipleComponent]
public class OwnerNetworkAnimator : NetworkAnimator // Taken straight from https://docs-multiplayer.unity3d.com/netcode/current/components/networkanimator/
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
