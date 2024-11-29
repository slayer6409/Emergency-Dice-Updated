using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using MysteryDice.Dice;
using Unity.Netcode;
using UnityEngine;
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
            if (StartOfRound.Instance.allPlayerScripts.Length == 1)
            {
                DieBehaviour.AllowedEffects[Random.Range(0, DieBehaviour.AllowedEffects.Count)].Use();
                return;
            }
            
            var playerScripts = new List<PlayerControllerB>(StartOfRound.Instance.allPlayerScripts);
            playerScripts = playerScripts.Where(x => !x.isPlayerDead && x.isActiveAndEnabled && x.isPlayerControlled).OrderBy(x => Random.value).ToList();

            ulong p1 = playerScripts[0].playerClientId;
            ulong p2 = playerScripts[1].playerClientId;

            Networker.Instance.makeLoverServerRPC(p1,p2);
        }
        public static void makeLovers(ulong p1, ulong p2)
        {
            var localPlayer = StartOfRound.Instance.localPlayerController;
            if (localPlayer.playerClientId == p1 || localPlayer.playerClientId == p2)
            {
                loverScript ls = localPlayer.gameObject.AddComponent<loverScript>();
                ls.me = localPlayer;
                var lover = localPlayer.playerClientId == p1 ? Misc.GetPlayerByUserID(p2) : Misc.GetPlayerByUserID(p1);
                ls.lover = lover;
                Misc.SafeTipMessage("Lovers!",$"You are now in love with {lover.playerUsername}");
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
    }
}
