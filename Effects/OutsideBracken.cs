using HarmonyLib;
using MysteryDice.Patches;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class OutsideBracken : IEffect
    {
        public string Name => "Outside bracken";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spawned a bracken outside!";

        public void Use()
        {
            Networker.Instance.OutsideBrackenServerRPC();
        }

        public static void SpawnOutsideBracken()
        {
            if (GetEnemies.Bracken == null)
                return;

            RoundManager.Instance.currentOutsideEnemyPower = 0;
            Misc.SpawnEnemyForced(GetEnemies.Bracken, 1, true);
            Networker.Instance.StartCoroutine(DelayedBrackenTeleportOutside());
        }

        public static IEnumerator DelayedBrackenTeleportOutside()
        {
            yield return new WaitForSeconds(3f);

            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (enemy is FlowermanAI)
                {
                    FlowermanAI bracken = (FlowermanAI)enemy;
                    EntranceTeleport[] allEntrances = GameObject.FindObjectsOfType<EntranceTeleport>();
                    EntranceTeleport mainEntrance = null;
                    for (int i = 0; i < allEntrances.Length; i++)
                    {
                        if (allEntrances[i].isEntranceToBuilding)
                        {
                            mainEntrance = allEntrances[i];
                            break;
                        }
                    }
                    if (mainEntrance == null) yield break;

                    

                    bracken.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                    bracken.isOutside = true;
                    bracken.ChangeOwnershipOfEnemy(GameNetworkManager.Instance.localPlayerController.actualClientId);
                    bracken.serverPosition = mainEntrance.entrancePoint.position;
                    bracken.transform.position = bracken.serverPosition;
                    bracken.agent.Warp(bracken.serverPosition);
                    bracken.SyncPositionToClients();
                    bracken.EnableEnemyMesh(true, false);
                }
            }
        }

        public static void SetNavmeshBrackenClient()
        {
            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (enemy is FlowermanAI)
                {
                    FlowermanAI bracken = (FlowermanAI)enemy;
                    bracken.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                }
            }
        }
    }
}
