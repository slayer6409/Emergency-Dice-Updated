using GameNetcodeStuff;
using UnityEngine;

namespace MysteryDice.MiscStuff;

public class PlayerTracker : MonoBehaviour
{
    PlayerControllerB trackedPlayer;
    private Vector3 offset;

    public void init(PlayerControllerB player, Vector3 offset)
    {
        trackedPlayer = player;
        this.offset = offset;
    }

    public void Update()
    {
        if (trackedPlayer != null)
        {
            transform.position = trackedPlayer.transform.position + offset;
        }
    }
}