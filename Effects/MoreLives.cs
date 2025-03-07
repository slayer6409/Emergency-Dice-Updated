using MysteryDice.Patches;
using System;
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class MoreLives : IEffect
    {
        public string Name => "Extra Lives";
        public EffectType Outcome => EffectType.Good;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Everyone Gets an Extra Life";

        public void Use()
        {
            Networker.Instance.AddLifeAllServerRPC();
        }
    }

    public class playerLifeController : MonoBehaviour
    {
        public int livesRemaining = 0;
        public PlayerControllerB player;
        public bool currentlyRunning = false;

        public void addLife()
        {
            livesRemaining++;
            Networker.Instance.SendMessageServerRPC(player.actualClientId,"Extra Life!", "You got an extra life! You now have " + livesRemaining + " lives remaining.");
        }
        private void FixedUpdate()
        {
            if (player.isPlayerDead && !currentlyRunning)
            {
                if (livesRemaining > 0)
                {
                    currentlyRunning = true;
                    StartCoroutine(wait()); 
                }
            }   
        }
        

        public IEnumerator wait()
        {
            livesRemaining--;
            yield return new WaitForSeconds(0.25f);
            Networker.Instance.RevivePlayerServerRpc(player.actualClientId, StartOfRound.Instance.middleOfShipNode.position);
            Networker.Instance.SendMessageServerRPC(player.actualClientId,"Revived!", "You used an extra life! You now have " + livesRemaining + " lives remaining.");
            StartCoroutine(wait2());
            
        }
        public IEnumerator wait2()
        {
            yield return new WaitForSeconds(0.25f);
            currentlyRunning = false;
            
        }
    }
}
