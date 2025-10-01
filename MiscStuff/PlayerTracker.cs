using System;
using Dawn.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace MysteryDice.MiscStuff;

public class PlayerTracker : MonoBehaviour
{
    public PlayerControllerB trackedPlayer;
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

public class RainbowScanNode : MonoBehaviour
{
    private ForceScanColorOnItem scanNodeForcer;

    private void Awake()
    {
        scanNodeForcer = GetComponentInChildren<ForceScanColorOnItem>();
    }
    
    public void Update()
    {
        float hue = Mathf.Repeat(Time.time * 0.2f, 1f); 
        Color borderColor = Color.HSVToRGB(hue, 1f, 1f);
        
        scanNodeForcer.borderColor = borderColor;
        
        float oppositeHue = Mathf.Repeat(hue + 0.5f, 1f);
        scanNodeForcer.textColor = Color.HSVToRGB(oppositeHue, 1f, 1f);
    }
}
