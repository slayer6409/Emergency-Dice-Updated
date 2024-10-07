using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice
{
    internal class Misc
    {
        public static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool isInside, bool isInvisible = false)
        {
            if (!Networker.Instance.IsHost) return;
            RoundManager RM = RoundManager.Instance;
            
            if (isInside)
            {
                if (isInvisible)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                        GameObject enemyObject = UnityEngine.Object.Instantiate(
                            enemy.enemyType.enemyPrefab,
                            randomVent.floorNode.position,
                            Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                        SetObjectInvisible(enemyObject);
                        enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                        RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
                    }
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                        RM.SpawnEnemyOnServer(randomVent.floorNode.position, randomVent.floorNode.eulerAngles.y, RM.currentLevel.Enemies.IndexOf(enemy));
                    }
                }
            }
            else
            {
                for(int i = 0; i < amount; i++)
                {
                    SpawnOutsideEnemy(enemy);
                }
            }
        }
        public static void SetObjectInvisible(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    // Ensure the material is using the Standard Shader
                    material.shader = Shader.Find("Standard");

                    // Set the rendering mode to Transparent
                    material.SetFloat("_Mode", 3);  // 3 corresponds to Transparent mode in Unity's Standard Shader
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;

                    // Set the alpha to 0 to make it invisible
                    Color color = material.color;
                    color.a = 0.1f;
                    material.color = color;
                }
            }
        }
        public static void SpawnOutsideEnemy(SpawnableEnemyWithRarity enemy)
        {
            RoundManager RM = RoundManager.Instance;

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
            GameObject[] aiNodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            aiNodes = aiNodes.OrderBy(x => Vector3.Distance(x.transform.position, Vector3.zero)).ToArray();

            Vector3 position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            position = RM.GetRandomNavMeshPositionInBoxPredictable(position, 30f, default(NavMeshHit), random) + Vector3.up;

            GameObject enemyObject = UnityEngine.Object.Instantiate(
                enemy.enemyType.enemyPrefab,
                position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));

            enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
        }

        /// <summary>
        /// Allows an enemy to spawn on a moon which he isnt native to.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="amount"></param>
        /// <param name="isInside"></param>
        public static void SpawnEnemyForced(SpawnableEnemyWithRarity enemy, int amount, bool isInside, bool isInvisible=false)
        {
            if (!RoundManager.Instance.currentLevel.Enemies.Contains(enemy))
            {
                RoundManager.Instance.currentLevel.Enemies.Add(enemy);
                SpawnEnemy(enemy, amount, isInside, isInvisible);
                RoundManager.Instance.currentLevel.Enemies.Remove(enemy);
            }
            else
            {
                SpawnEnemy(enemy, amount, isInside, isInvisible);
            }
        }

        public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static PlayerControllerB GetPlayerByUserID(ulong userID)
        {
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (player.playerClientId == userID)
                    return player;
            }
            return null;
        }


        public static NetworkObjectReference SpawnEnemyOnServer(Vector3 spawnPosition, float yRot, SpawnableEnemyWithRarity enemy)
        {
            NetworkObjectReference emptyReference = new NetworkObjectReference();
            if (!Networker.Instance.IsServer)
                return emptyReference;

            GameObject gameObject = UnityEngine.Object.Instantiate(enemy.enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0f, yRot, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            return gameObject.GetComponentInChildren<NetworkObject>();
        }

        public static void ChatWrite(string chatMessage)
        {
            HUDManager.Instance.lastChatMessage = chatMessage;
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 4f);
            if (HUDManager.Instance.ChatMessageHistory.Count >= 4)
            {
                HUDManager.Instance.chatText.text.Remove(0, HUDManager.Instance.ChatMessageHistory[0].Length);
                HUDManager.Instance.ChatMessageHistory.Remove(HUDManager.Instance.ChatMessageHistory[0]);
            }
            string text = $"<color=#00ffff>{chatMessage}</color>";
            HUDManager.Instance.ChatMessageHistory.Add(text);
            HUDManager.Instance.chatText.text = "";
            for (int i = 0; i < HUDManager.Instance.ChatMessageHistory.Count; i++)
            {
                TextMeshProUGUI textMeshProUGUI = HUDManager.Instance.chatText;
                textMeshProUGUI.text = textMeshProUGUI.text + "\n" + HUDManager.Instance.ChatMessageHistory[i];
            }
        }

        public static void SafeTipMessage(string title, string body)
        {
            try
            {
                HUDManager.Instance.DisplayTip(title, body);
            }
            catch
            {
                MysteryDice.CustomLogger.LogWarning("There's a problem with the DisplayTip method. This might have happened due to a new game verison, or some other mod.");
                try
                {
                    ChatWrite($"{title}: {body}");
                }
                catch
                {
                    MysteryDice.CustomLogger.LogWarning("There's a problem with writing to the chat. This might have happened due to a new game verison, or some other mod.");
                }
            }

        }

        public static PlayerControllerB GetRandomAlivePlayer()
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }

            return validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
        }

        public static bool IsPlayerAliveAndControlled(PlayerControllerB player)
        {
            return !player.isPlayerDead &&
                    player.isActiveAndEnabled &&
                    player.IsSpawned &&
                    player.isPlayerControlled;
        }

        public static void AdjustWeight(ulong userID, float factor)
        {
            PlayerControllerB player = null;
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB playerComp = playerPrefab.GetComponent<PlayerControllerB>();
                if (playerComp.playerClientId == userID)
                {
                    player = playerComp;
                    break;
                }
            }
            if (player == null)
            {
                Debug.LogError("Player not found.");
                return;
            }


            float totalWeight = 0;
            float totalWeight2 = 0;

            foreach (var item in player.ItemSlots)
            {
                if (item == null || item.itemProperties.weight == 0) continue;

                float clampedWeight = Mathf.Clamp(item.itemProperties.weight - 1f, 0.0f, 100f);
                float scaledWeight = Mathf.RoundToInt(clampedWeight * 105f);

                scaledWeight *= factor;

                totalWeight2 += scaledWeight;

                float carryWeight = Mathf.Clamp((scaledWeight / 105f) + 1f, 1f, 10f);

                item.itemProperties.weight = carryWeight;

                //MysteryDice.CustomLogger.LogWarning($"Item weight: {item.itemProperties.weight}, Scaled weight: {scaledWeight}, Carry weight: {carryWeight}");
                
                totalWeight += carryWeight;
            }
            //MysteryDice.CustomLogger.LogWarning($"Total Weight before setting: {totalWeight}");
            //MysteryDice.CustomLogger.LogWarning($"Total Weight2 before setting: {totalWeight2}");
            float e = Mathf.Clamp((totalWeight2 / 105f) + 1f, 1f, 10f);
            if (e != 0)
            {
                player.carryWeight = e;
                //MysteryDice.CustomLogger.LogWarning($"Player carry weight updated to: {player.carryWeight}");

            }
        }
    }
}

