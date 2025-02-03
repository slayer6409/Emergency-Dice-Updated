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

        public void addLife()
        {
            livesRemaining++;
            Misc.SafeTipMessage("Extra Life!", "You got an extra life! You now have " + livesRemaining + " lives remaining.");
        }
        private void FixedUpdate()
        {
            if (player.isPlayerDead)
            {
                if (livesRemaining > 0)
                {
                    StartCoroutine(wait());
                }
            }   
        }

        public IEnumerator wait()
        {
            yield return new WaitForSeconds(0.25f);
            livesRemaining--;
            Networker.Instance.RevivePlayerServerRpc(player.actualClientId, StartOfRound.Instance.middleOfShipNode.position);
            Misc.SafeTipMessage("Revived!", "You used an extra life! You now have " + livesRemaining + " lives remaining.");
        }
    }
}
