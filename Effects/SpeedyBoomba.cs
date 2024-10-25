using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using LethalThings;
using HarmonyLib;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class SpeedyBoomba : IEffect
    {
        public string Name => "Speedy Boomba";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Gotta Boom Fast";

        public void Use()
        {
            
            if (GetEnemies.Boomba == null)
                return;
            var list = Misc.SpawnEnemyForced2(GetEnemies.Boomba, 1, true, returnObject:true);
            for (int i = 0; i < list.Count; i++) 
            {
                var enemy = list[i];
                if (enemy == null) continue; 
                enemy.AddComponent<CustomRoombaSpeed>().useCustomSpeed = true;
            }

        }
    }
    //[HarmonyPatch(typeof(RoombaAI))]
    //[HarmonyPatch("Update")]
    //class RoombaAI_Update_Patch
    //{
    //    static void Postfix(RoombaAI __instance)
    //    {
    //        var customSpeed = __instance.GetComponent<CustomRoombaSpeed>();
    //        if (customSpeed != null && customSpeed.useCustomSpeed)
    //        {
    //            NavMeshAgent agent = __instance.GetComponent<NavMeshAgent>();
    //            if (agent != null)
    //            {
    //                agent.speed = MysteryDice.BoombaEventSpeed.Value; 
    //            }
    //        }
    //    }
    //}
    public class CustomRoombaSpeed : MonoBehaviour
    {
        public bool useCustomSpeed = false;
    }
}
