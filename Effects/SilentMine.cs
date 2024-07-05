using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class SilentMine : IEffect
    {
        public string Name => "Silent mine";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Mines are silent and invisible";

        public void Use()
        {
            Networker.Instance.SilenceMinesServerRPC();
        }

        public static IEnumerator SilenceAllMines(bool isServer)
        {
            if(isServer)
                MineOverflow.SpawnMoreMines(10);

            yield return new WaitForSeconds(5); //lazy fix to allow all clients to sync

            Landmine[] landmines = GameObject.FindObjectsOfType<Landmine>();

            foreach(Landmine mine in landmines)
            {
                mine.mineAudio.volume = 0f;
                mine.mineAnimator.StopPlayback();
                Renderer[] renderers = mine.GetComponentsInChildren<Renderer>();

                foreach(Renderer renderer in renderers)
                {
                    GameObject.Destroy(renderer);
                }   
            }
        }
       
    }
}
