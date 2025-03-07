using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class OopsAllBlank : IEffect
    {
        public string Name => "Oops All Blank";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Oops All *";
      
        public void Use()
        {
            Networker.Instance.OopsPlayerServerRPC();
        }

    }

    public class oopsController : MonoBehaviour
    {
        public UnlockableSuit currentSuit { get; set; }
        public UnlockableSuit suitToSwitchTo { get; set; }
        public PlayerControllerB player { get; set; }
        public void FixedUpdate()
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded)
            {
                fixStuff();
                return;
            }
        } 
        public void fixStuff()
        {
            currentSuit.SwitchSuitToThis(player);
            Destroy(this);
        }

        public void switchSuit()
        {
            suitToSwitchTo.SwitchSuitToThis(player);
        }
    }
}
