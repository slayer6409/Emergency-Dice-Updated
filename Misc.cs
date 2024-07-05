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
        public static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool isInside)
        {
            if (!Networker.Instance.IsHost) return;
            RoundManager RM = RoundManager.Instance;

            if (isInside)
            {
                for (int i = 0; i < amount; i++)
                {
                    EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                    RM.SpawnEnemyOnServer(randomVent.floorNode.position, randomVent.floorNode.eulerAngles.y, RM.currentLevel.Enemies.IndexOf(enemy));
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
        public static void SpawnEnemyForced(SpawnableEnemyWithRarity enemy, int amount, bool isInside)
        {
            if (!RoundManager.Instance.currentLevel.Enemies.Contains(enemy))
            {
                RoundManager.Instance.currentLevel.Enemies.Add(enemy);
                SpawnEnemy(enemy, amount, isInside);
                RoundManager.Instance.currentLevel.Enemies.Remove(enemy);
            }
            else
            {
                SpawnEnemy(enemy, amount, isInside);
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
    }
}
