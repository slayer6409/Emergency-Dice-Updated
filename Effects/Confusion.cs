using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Confusion : IEffect
    {
        public string Name => "Confusion";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "??????????????????????";
      
        public void Use()
        {
            Networker.Instance.ConfusionPlayerServerRPC(false);
        }

    }
    internal class StupidConfusion : IEffect
    {
        public string Name => "Stupid Confusion";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "??????????????????????????????????????????????????";
      
        public void Use()
        {
            
            Networker.Instance.ConfusionPlayerServerRPC(true);
        }
    }

    public class confusionPlayer : MonoBehaviour
    {
        public UnlockableSuit currentSuit { get; set; }
        public PlayerControllerB player { get; set; }
        public int timer = 30;
        public bool isTimerRunning = false;
        public bool stupidMode = false;

        public void FixedUpdate()
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded)
            {
                fixStuff();
                return;
            }
            if (!isTimerRunning)
            {
                isTimerRunning = true;
                StartCoroutine(waitForTime());
            }
        }

        public void fixStuff()
        {
            currentSuit.SwitchSuitToThis(player);
            Destroy(this);
        }
        public IEnumerator waitForTime()
        {
            if(!stupidMode) yield return new WaitForSeconds(timer);
            else yield return new WaitForSeconds(1);
            ChangeStuff();
        }

        public void ChangeStuff()
        {
            Networker.orderedSuits[Random.Range(0, Networker.orderedSuits.Count)].SwitchSuitToThis(player);
            isTimerRunning = false;
        }
    }
}
