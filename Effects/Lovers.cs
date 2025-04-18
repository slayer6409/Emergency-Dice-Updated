using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using MysteryDice.Dice;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Lovers : IEffect
    {
        public string Name => "Lovers";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "2 people are now in love!";

        public void Use()
        {
            
            if (Misc.playerCount()==1)
            {
                DieBehaviour.AllowedEffects[Random.Range(0, DieBehaviour.AllowedEffects.Count)].Use();
                return;
            }
            
            var playerScripts = new List<PlayerControllerB>(StartOfRound.Instance.allPlayerScripts);
            playerScripts = playerScripts.Where(x => !x.isPlayerDead && x.isActiveAndEnabled && x.isPlayerControlled).OrderBy(x => Random.value).ToList();

            int p1 = Array.IndexOf(StartOfRound.Instance.allPlayerScripts,playerScripts[0]);
            int p2 = Array.IndexOf(StartOfRound.Instance.allPlayerScripts,playerScripts[1]);

            Networker.Instance.makeLoverServerRPC(p1,p2);
        }
        public static void makeLovers(int p1, int p2)
        {
            var localPlayer = StartOfRound.Instance.localPlayerController;
            if (localPlayer == StartOfRound.Instance.allPlayerScripts[p1] || localPlayer == StartOfRound.Instance.allPlayerScripts[p2])
            {
                loverScript ls = localPlayer.gameObject.AddComponent<loverScript>();
                ls.me = localPlayer;
                var lover = localPlayer == StartOfRound.Instance.allPlayerScripts[p1] ? Misc.GetPlayerByUserID(p2) : Misc.GetPlayerByUserID(p1);
                ls.lover = lover;
                Misc.SafeTipMessage("Lovers!",$"You are now in love with {lover.playerUsername}");
            }
        }

        public static void removeLovers()
        {
            var localPlayer = StartOfRound.Instance.localPlayerController;
            var ls = localPlayer.GetComponent<loverScript>();
            var bl = localPlayer.GetComponent<badLoverScripta>();
            if (ls != null)
            {
                Misc.SafeTipMessage("Lovers",$"You are no longer in love with {ls.lover.playerUsername}");
                ls.removeLover();
            }
            if (bl != null)
            {
                Misc.SafeTipMessage("Lovers", "You are no longer in love with an Enemy");
                bl.removeLover();
            }
        }
    }

    public class loverScript : MonoBehaviour
    {
        public PlayerControllerB me;
        public PlayerControllerB lover;
        public void Update()
        {
            if (lover.isPlayerDead)
            {
                me.KillPlayer(Vector3.up, true, CauseOfDeath.Abandoned,0);
                Destroy(this);
            }
            if(me.isPlayerDead)Destroy(this);
        }

        public void removeLover()
        {
            Destroy(this);
        }
    } 
   
}
