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
    public class Misc
    {
        public static Item GetItemByName(string itemName, bool matchCase = true)
        {
            StringComparison comparisonType = ((!matchCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
            foreach (Item items in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (items.itemName.Equals(itemName, comparisonType))
                {
                    return items;
                }
            }
            foreach (SpawnableItemWithRarity items in RoundManager.Instance.currentLevel.spawnableScrap)
            {
                if (items.spawnableItem.itemName.Equals(itemName, comparisonType))
                {
                    return items.spawnableItem;
                }
            }
            
            return null;
        }
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
        public static void SpawnEnemy(EnemyType enemy, int amount, bool isInside, bool isInvisible = false)
        {
            if (!Networker.Instance.IsHost) return;
            
            RoundManager RM = RoundManager.Instance;
            
            Vector3 position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            
            EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
            
            if (isInside) position = randomVent.floorNode.position; 
            
            for (int i = 0; i < amount; i++)
            {
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    enemy.enemyPrefab,
                    position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                if (isInvisible) SetObjectInvisible(enemyObject);
                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            }
        }

        public static bool canDiceYet()
        {
            if (StartOfRound.Instance == null) return false;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return false;
            return true;
        }

        public static int playerCount()
        {
            int players = 0;
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if(IsPlayerAliveAndControlled(player)) players++;
            }
            return players;
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

        public static PlayerControllerB getPlayerBySteamID(ulong steamID)
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (IsPlayerReal(player))
                    validPlayers.Add(player);
            }
            return validPlayers.Where(x=>x.playerSteamId == steamID).FirstOrDefault();
        }
        public static int getIntPlayerID(ulong playerID)
        {
            int index = -1; 

            for (int i = 0; i < StartOfRound.Instance.allPlayerObjects.Count(); i++)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>();
                if (player.isPlayerDead||player.isPlayerControlled)
                    if (player.actualClientId == playerID)
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        public static ulong getPlayerIDFromInt(int playerID)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerID];
            return player.actualClientId;
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
        public static List<GameObject> SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool isInside, bool isInvisible = false, bool returnObject = false)
        {
            List<GameObject> list = new List<GameObject>();
            if (!Networker.Instance.IsHost) return list;
            RoundManager RM = RoundManager.Instance;

            if (isInside)
            {
                for (int i = 0; i < amount; i++)
                {
                    EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
                    GameObject enemyObject = UnityEngine.Object.Instantiate(
                        enemy.enemyType.enemyPrefab,
                        randomVent.floorNode.position,
                        Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                    if (isInvisible) SetObjectInvisible(enemyObject);
                    enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                    RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
                    list.Add(enemyObject);
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    list.Add(SpawnOutsideEnemy(enemy, true));
                }
            }
            return list;
        }
        public static GameObject SpawnOutsideEnemy(SpawnableEnemyWithRarity enemy, bool returnObject)
        {
            List<GameObject> result = new List<GameObject>();
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
            return enemyObject;
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
                SpawnEnemy(enemy.enemyType, amount, isInside, isInvisible);
                RoundManager.Instance.currentLevel.Enemies.Remove(enemy);
            }
            else
            {
                SpawnEnemy(enemy.enemyType, amount, isInside, isInvisible);
            }
        }
        public static List<GameObject> SpawnEnemyForced2(SpawnableEnemyWithRarity enemy, int amount, bool isInside, bool isInvisible=false, bool returnObject = false)
        {
            List<GameObject> result = new List<GameObject>();
            if (!RoundManager.Instance.currentLevel.Enemies.Contains(enemy))
            {
                RoundManager.Instance.currentLevel.Enemies.Add(enemy);
                result = SpawnEnemy(enemy, amount, isInside, isInvisible,returnObject:true);
                RoundManager.Instance.currentLevel.Enemies.Remove(enemy);
            }
            else
            {
                result = SpawnEnemy(enemy, amount, isInside, isInvisible, returnObject: true);
            }
            return result;
        }

        public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static PlayerControllerB GetPlayerByUserID(ulong userID)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.actualClientId == userID)
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

        public static trap[] getAllTraps()
        {
            List<trap> allTraps = new List<trap>();
            List<SpawnableMapObject> allMapTraps = StartOfRound.Instance.levels
                .SelectMany(level => level.spawnableMapObjects)
                .GroupBy(x => x.prefabToSpawn.name)
                .Select(g => g.First())
                .ToList();
            foreach (SpawnableMapObject spawnableMapObject in allMapTraps)
            {
                allTraps.Add(new trap(spawnableMapObject.prefabToSpawn.name, spawnableMapObject.prefabToSpawn));
            }
            if (MysteryDice.CodeRebirthPresent)
            {
                foreach (var crt in CodeRebirthCheckConfigs.getSpawnPrefabs())
                {
                    if(allTraps.Exists(x=>x.name==crt.name)) continue;
                    allTraps.Add(crt);
                }
            }
            return allTraps.ToArray();
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

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }

            if (validPlayers.Count == 1) return validPlayers[0];

            return validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
        }
        public static ulong GetRandomPlayerID()
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }
            if(validPlayers.Count==1) return validPlayers[0].actualClientId;

            return validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)].actualClientId;
        } 
        public static PlayerControllerB GetRandomPlayer()
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (IsPlayerReal(player))
                    validPlayers.Add(player);
            }
            if(validPlayers.Count==1) return validPlayers[0];

            return validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
        }

        public static bool IsPlayerAliveAndControlled(PlayerControllerB player)
        {
            return !player.isPlayerDead &&
                    player.isPlayerControlled;
        }
        public static bool IsPlayerReal(PlayerControllerB player)
        {
            return player.isActiveAndEnabled ||
                   player.isPlayerDead;
        }

        public static SpawnableEnemyWithRarity getEnemyByName(string name)
        {
            var enemySet = new HashSet<SpawnableEnemyWithRarity>();
            foreach (var level in StartOfRound.Instance.levels)
            {
                enemySet.UnionWith(level.Enemies);
                enemySet.UnionWith(level.OutsideEnemies);
                enemySet.UnionWith(level.DaytimeEnemies);
            }

            var uniqueEnemies = enemySet
                .GroupBy(x => x.enemyType.enemyName)
                .Select(g => g.First())
                .OrderBy(x => x.enemyType.enemyName)
                .ToList();
            return uniqueEnemies.FirstOrDefault(x => x.enemyType.enemyName == name);
        }
        public static void AdjustWeight(ulong userID, float factor)
        {
            PlayerControllerB player = null;
            foreach (PlayerControllerB playerComp in StartOfRound.Instance.allPlayerScripts)
            {
                if (playerComp.actualClientId == userID)
                {
                    player = playerComp;
                    break;
                }
            }
            if (player == null)
            {
                MysteryDice.CustomLogger.LogError("Player not found.");
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

    public class trap
    {
        public string name;
        public GameObject prefab;

        public trap(string _name, GameObject _prefab)
        {
            name = _name;
            prefab = _prefab;
        }
    }
}

