using GameNetcodeStuff;
using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class PerformRandomEmote : IEffect
    {
        public string Name => "Random Emote Time";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Random Emote Time!!!";
        public void Use()
        {
            Networker.Instance.RequestEmotesInRadiusServerRpc(StartOfRound.Instance.localPlayerController.transform.position, 20);
        }

        [MethodImpl (MethodImplOptions.NoInlining)]
        public static void PerformEmote()
        {
            var emotes = TooManyEmotes.UI.EmoteMenu.currentLoadoutEmotesList;
            if (emotes == null || emotes.Count == 0) return;

            var emote = emotes[UnityEngine.Random.Range(0, emotes.Count)];
            if (emote.inEmoteSyncGroup && emote.emoteSyncGroup.Count > 0)
                emote = emote.emoteSyncGroup[0];
            TooManyEmotes.EmoteControllerPlayer.emoteControllerLocal.TryPerformingEmoteLocal(emote);
        }
        
    }
}
