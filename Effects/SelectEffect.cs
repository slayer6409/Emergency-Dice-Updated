using System;
using GameNetcodeStuff;
using MysteryDice.Dice;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using Newtonsoft.Json;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;
using Color = UnityEngine.Color;

namespace MysteryDice.Effects
{
    internal class SelectEffect : IEffect
    {
        public static bool ReviveOpen = false;
        public string Name => "Select Effect";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can select an effect";

        public static GameObject EffectMenu = null;

        public static bool isSpecial()
        {
            return StartOfRound.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID ||
                   StartOfRound.Instance.localPlayerController.playerSteamId == 76561199094139351 ||
                   StartOfRound.Instance.localPlayerController.playerSteamId == 76561198086086035;
        }

        public void Use()
        {
            if(MysteryDice.NewDebugMenu.Value) DebugMenuStuff.ShowSelectEffectMenu();
            else ShowSelectMenu(false,false,fromSaint:true);
        }
        public static void showDebugMenu(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }
            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);


            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            exitButton.onClick.AddListener(() =>
            {
                CloseSelectMenu();
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Choose a Category";
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"Select Effect";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                ShowSelectMenu(full, complete,su);
            });


           

            GameObject effectObj6 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText6 = effectObj6.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText6.text = $"Spawn Stuff";
            Button button6 = effectObj6.GetComponent<Button>();
            button6.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnFunctions(full, complete, su);
            });

            
            GameObject effectObj7 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText7 = effectObj7.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText7.text = $"Player Functions";

            Button button7 = effectObj7.GetComponent<Button>();
            button7.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                playerFunctions(full, complete, su);
            });
            
                       
            if (isSpecial())
            {
                
                GameObject effectObj9 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText9 = effectObj9.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText9.text = $"Special Functions";

                Button button9 = effectObj9.GetComponent<Button>();
                button9.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    specialFunctions(full, complete,su);
                });
            }
            if (su)
            {
                GameObject effectObj8 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText8.text = $"Grant/Revoke Admin";

                Button button8 = effectObj8.GetComponent<Button>();
                button8.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    grantAdmin(full, complete, su);
                });
            }

           
        }
        public static List<IEffect> getOrdered(bool full, bool complete = false)
        {
            
            List<IEffect> effects = new List<IEffect>(complete
                ? DieBehaviour.CompleteEffects
                : (full ? DieBehaviour.AllEffects : DieBehaviour.AllowedEffects));

            if (StartOfRound.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID ||
                StartOfRound.Instance.localPlayerController.playerSteamId == 76561199094139351 ||
                StartOfRound.Instance.localPlayerController.playerSteamId == 76561198984467725)
            {
                effects.Add(new ForceTakeoff());
            }

            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteEffectNames = favoritesData.Favorites;

            List<IEffect> sortedEffects = effects
                .OrderByDescending(effect => favoriteEffectNames.Contains(effect.Name))  // Favorites first
                .ThenBy(effect => effect.Name, StringComparer.OrdinalIgnoreCase)         // Sort alphabetically
                .ToList();

            return sortedEffects;
        }
        public static void spawnEnemy(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn an Enemy";
            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                spawnFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Load favorites
            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteEnemyNames = favoritesData.FavoriteEnemies;

            List<SpawnableEnemyWithRarity> allEnemies = StartOfRound.Instance.levels
                .SelectMany(level => level.Enemies.Union(level.OutsideEnemies).Union(level.DaytimeEnemies))
                .GroupBy(x => x.enemyType.enemyName)
                .Select(g => g.First())
                .ToList();

            allEnemies = allEnemies.OrderBy(e => favoriteEnemyNames.Contains(e.enemyType.enemyName) ? 0 : 1)
                                   .ThenBy(e => e.enemyType.enemyName)
                                   .ToList();

            foreach (var enemy in allEnemies)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                bool isFavorite = favoriteEnemyNames.Contains(enemy.enemyType.enemyName);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {enemy.enemyType.enemyName}";
                buttonText.color = isFavorite ? Color.gray : Color.black;

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();

                    if (StartOfRound.Instance.localPlayerController.isPlayerDead) 
                    {
                        Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyType.enemyName, StartOfRound.Instance.spectateCamera.transform.position);
                    }
                    else
                    {
                        Networker.Instance.CustomMonsterServerRPC(enemy.enemyType.enemyName, 1, 1, StartOfRound.Instance.localPlayerController.isInsideFactory);
                    }
                    string outsideInside = StartOfRound.Instance.localPlayerController.isInsideFactory ? "Inside" : "Outside";
                    string txtToSay = $"Spawned {enemy.enemyType.enemyName} {outsideInside}";
                    Misc.SafeTipMessage($"Spawned {enemy.enemyType.enemyName}", txtToSay);
                });

                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = enemy.enemyType.enemyName;
                rightClickHandler.category = "FavoriteEnemies";
                rightClickHandler.full = full;
                rightClickHandler.complete = complete;
            }
        }
        public static void spawnMiniEnemy(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn an Mini Enemy";
            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                specialFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Load favorites
            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteEnemyNames = favoritesData.FavoriteEnemies;

            List<SpawnableEnemyWithRarity> allEnemies = StartOfRound.Instance.levels
                .SelectMany(level => level.Enemies.Union(level.OutsideEnemies).Union(level.DaytimeEnemies))
                .GroupBy(x => x.enemyType.enemyName)
                .Select(g => g.First())
                .ToList();

            allEnemies = allEnemies.OrderBy(e => favoriteEnemyNames.Contains(e.enemyType.enemyName) ? 0 : 1)
                                   .ThenBy(e => e.enemyType.enemyName)
                                   .ToList();

            foreach (var enemy in allEnemies)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                bool isFavorite = favoriteEnemyNames.Contains(enemy.enemyType.enemyName);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {enemy.enemyType.enemyName}";
                buttonText.color = isFavorite ? Color.gray : Color.black;

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();

                    if (StartOfRound.Instance.localPlayerController.isPlayerDead) 
                    {
                        Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyType.enemyName, StartOfRound.Instance.spectateCamera.transform.position,true, new Vector3(0.25f,0.25f,0.25f));
                    }
                    else
                    {
                        Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyType.enemyName, StartOfRound.Instance.localPlayerController.transform.position,true, new Vector3(0.25f,0.25f,0.25f));
                    }
                    string outsideInside = StartOfRound.Instance.localPlayerController.isInsideFactory ? "Inside" : "Outside";
                    string txtToSay = $"Spawned {enemy.enemyType.enemyName} {outsideInside}";
                    Misc.SafeTipMessage($"Spawned {enemy.enemyType.enemyName}", txtToSay);
                });

                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = enemy.enemyType.enemyName;
                rightClickHandler.category = "FavoriteEnemies";
                rightClickHandler.full = full;
                rightClickHandler.complete = complete;
            }
        }

        public static void spawnTrap(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn a Trap";
            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                spawnFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Load favorites
            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteTrapNames = favoritesData.FavoriteTraps;

            List<SpawnableMapObject> allTraps = StartOfRound.Instance.levels
                .SelectMany(level => level.spawnableMapObjects)
                .GroupBy(x => x.prefabToSpawn.name)
                .Select(g => g.First())
                .ToList();

            // ✅ Sort: Favorites first, then others alphabetically
            allTraps = allTraps.OrderBy(t => favoriteTrapNames.Contains(t.prefabToSpawn.name) ? 0 : 1)
                               .ThenBy(t => t.prefabToSpawn.name)
                               .ToList();

            foreach (var trap in allTraps)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                bool isFavorite = favoriteTrapNames.Contains(trap.prefabToSpawn.name);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {trap.prefabToSpawn.name}";
                buttonText.color = isFavorite ? Color.gray : Color.black;

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();

                    Vector3 spawnPosition;
                    if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                    {
                        spawnPosition = StartOfRound.Instance.spectateCamera.transform.position; // ✅ Spectator Fix
                    }
                    else
                    {
                        spawnPosition = StartOfRound.Instance.localPlayerController.transform.position; // ✅ Normal Player Spawn
                    }

                    Networker.Instance.spawnTrapOnServerRPC(trap.prefabToSpawn.name, 1, 
                        StartOfRound.Instance.localPlayerController.isInsideFactory,
                        Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController),
                        StartOfRound.Instance.localPlayerController.isPlayerDead,
                        spawnPosition);
                    Misc.SafeTipMessage($"Spawned {trap.prefabToSpawn.name}", $"Spawned at {spawnPosition}");
                });

                // ✅ Right-click handler for favoriting
                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = trap.prefabToSpawn.name;
                rightClickHandler.category = "FavoriteTraps";
                rightClickHandler.full = full;
                rightClickHandler.complete = complete;
            }
        }
       public static void spawnScrap(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn Scrap";
            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                spawnFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Load favorites
            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteScrapNames = favoritesData.FavoriteScraps;

            List<SpawnableItemWithRarity> allScraps = StartOfRound.Instance.levels
                .SelectMany(level => level.spawnableScrap)
                .GroupBy(x => x.spawnableItem.itemName)
                .Select(g => g.First())
                .ToList();

            // ✅ Sort: Favorites first, then others alphabetically
            allScraps = allScraps.OrderBy(s => favoriteScrapNames.Contains(s.spawnableItem.itemName) ? 0 : 1)
                                 .ThenBy(s => s.spawnableItem.itemName)
                                 .ToList();

            foreach (var scrap in allScraps)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                bool isFavorite = favoriteScrapNames.Contains(scrap.spawnableItem.itemName);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {scrap.spawnableItem.itemName}";
                buttonText.color = isFavorite ? Color.gray : Color.black;

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();

                    Vector3 spawnPosition;
                    if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                    {
                        spawnPosition = StartOfRound.Instance.spectateCamera.transform.position; // ✅ Spectator Fix
                    }
                    else
                    {
                        spawnPosition = StartOfRound.Instance.localPlayerController.transform.position; // ✅ Normal Player Spawn
                    }

                    Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), 
                        1, scrap.spawnableItem.itemName, StartOfRound.Instance.localPlayerController.isPlayerDead, 
                        spawnPosition);
                    Misc.SafeTipMessage($"Spawned {scrap.spawnableItem.name}", $"Spawned at {spawnPosition}");
                });

                // ✅ Right-click handler for favoriting
                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = scrap.spawnableItem.itemName;
                rightClickHandler.category = "FavoriteScraps";
                rightClickHandler.full = full;
                rightClickHandler.complete = complete;
            }
        }
       public static void spawnWorldObject(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn Objects";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                showDebugMenu(full, complete,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            List<SpawnableOutsideObjectWithRarity> allObjects = new List<SpawnableOutsideObjectWithRarity>();

            foreach (var level in StartOfRound.Instance.levels)
            {
                allObjects = allObjects
                    .Union(level.spawnableOutsideObjects)
                    .ToList();
            }
            allObjects = allObjects
            .GroupBy(x => x.spawnableObject.name)
            .Select(g => g.First())
            .OrderBy(x => x.spawnableObject.name)
            .ToList();

            foreach (var scrap in allObjects)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{scrap.spawnableObject.name}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if(MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();
                    Networker.Instance.SpawnObjectServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), 1, scrap.spawnableObject.name);
                    string txtToSay = "";
                    txtToSay = $"Spawned {scrap.spawnableObject.name}";
                    Misc.SafeTipMessage($"Object Spawned", txtToSay);
                });
            }
        }
        public static void spawnShopItems(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn Shop Items";
            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                spawnFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Load favorites
            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteShopItemNames = favoritesData.FavoriteShopItems;

            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            List<Item> allShopItems = terminal.buyableItemsList
                .GroupBy(x => x.itemName)
                .Select(g => g.First())
                .ToList();

            // ✅ Sort: Favorites first, then others alphabetically
            allShopItems = allShopItems.OrderBy(s => favoriteShopItemNames.Contains(s.itemName) ? 0 : 1)
                                       .ThenBy(s => s.itemName)
                                       .ToList();

            foreach (var item in allShopItems)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                bool isFavorite = favoriteShopItemNames.Contains(item.itemName);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {item.itemName}";
                buttonText.color = isFavorite ? Color.gray : Color.black;

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();

                    Vector3 spawnPosition;
                    if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                    {
                        spawnPosition = StartOfRound.Instance.spectateCamera.transform.position; 
                    }
                    else
                    {
                        spawnPosition = StartOfRound.Instance.localPlayerController.transform.position; 
                    }

                    Networker.Instance.spawnStoreItemServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), 
                        item.itemName,spawnPosition);
                });

                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = item.itemName;
                rightClickHandler.category = "FavoriteShopItems";
            }
        }
        public static void grantAdmin(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Grant Admin";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                showDebugMenu(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 

            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            
            
            allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

            foreach (var player in allPlayers)
            {
                if(!Misc.IsPlayerReal(player))continue;
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{player.playerUsername}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    Networker.Instance.becomeAdminServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), false, false);
                    string txtToSay = "";
                    txtToSay = $"Made {player.playerUsername} an admin"; 
                    Networker.Instance.MessageToHostServerRPC($"Admin", txtToSay);
                });
            }
        }  
        public static void spawnFunctions(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Spawn Functions";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                showDebugMenu(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
            GameObject effectObj2 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText2.text = $"Spawn Enemy";

            Button button2 = effectObj2.GetComponent<Button>();
            button2.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnEnemy(full, complete,su);
            });
            GameObject effectObj3 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText3 = effectObj3.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText3.text = $"Spawn Scrap";

            Button button3 = effectObj3.GetComponent<Button>();
            button3.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnScrap(full, complete,su);
            });

            GameObject effectObj5 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText5 = effectObj5.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText5.text = $"Spawn Shop Items";

            Button button5 = effectObj5.GetComponent<Button>();
            button5.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnShopItems(full, complete,su);
            });

            GameObject effectObj4 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText4 = effectObj4.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText4.text = $"Spawn Trap";

            Button button4 = effectObj4.GetComponent<Button>();
            button4.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnTrap(full, complete,su);
            });
          
            if(isSpecial() || StartOfRound.Instance.localPlayerController.IsHost)
            {
                GameObject effectObj6 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText6 = effectObj6.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText6.text = $"Spawn Outside Objects";

                Button button6 = effectObj6.GetComponent<Button>();
                button6.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    spawnWorldObject(full, complete, su);
                });
            }
        }
        public static void specialFunctions(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Special Functions";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                showDebugMenu(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
            
            GameObject effectObj8 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText8.text = $"Play Sound";

            Button button8 = effectObj8.GetComponent<Button>();
            button8.onClick.AddListener(() =>
            {
                CloseSelectMenu(); 
                ShowSoundMenu(full, complete,su);
            });
            
            GameObject effectObj9 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText9 = effectObj9.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText9.text = $"Spawn miniture Enemy";

            Button button9 = effectObj9.GetComponent<Button>();
            button9.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                spawnMiniEnemy(full, complete,su);
            });
        }
        public static void playerFunctions(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Player Functions";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                showDebugMenu(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
            
            GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"Revive Player";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                RevivePlayer(full, complete, su);
            });
            GameObject effectObj2 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText2.text = $"Teleport to Player";

            Button button2 = effectObj2.GetComponent<Button>();
            button2.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                TeleportPlayer(full, complete, su);
            });
            
            GameObject effectObj3 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText3 = effectObj3.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText3.text = $"Bring Player";

            Button button3 = effectObj3.GetComponent<Button>();
            button3.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                TeleportPlayer(full, complete, su, bring:true);
            });

            if (isSpecial())
            {
                GameObject effectObj4 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText4 = effectObj4.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText4.text = $"Force Suit";

                Button button4 = effectObj4.GetComponent<Button>();
                button4.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    ForceSuitPlayerSelector(full, complete, su);
                });
            }
        }
        public static void TeleportPlayer(bool full, bool complete, bool su = false, bool bring = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Teleport Player";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                playerFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 

            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            
            allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

            
            foreach (var player in allPlayers)
            {
                if(!player.isPlayerControlled)continue;
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{player.playerUsername}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();
                    var player2 = StartOfRound.Instance.localPlayerController;
                    if (player2.isPlayerDead && bring)
                    {
                        Networker.Instance.TeleportOrBringPlayerToPosServerRPC(StartOfRound.Instance.spectateCamera.transform.position, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                    }
                    else
                    {
                        Networker.Instance.TeleportOrBringPlayerServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController), Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), bring);
                    }
                    
                    string txtToSay = "";
                    if (bring)
                    {
                        txtToSay = $"Brought {player.playerUsername} to you!";
                    }
                    else
                    {
                        txtToSay = $"Teleported to {player.playerUsername}"; 
                    }
                    Misc.SafeTipMessage($"Teleport", txtToSay);
                });
                
            }
        } 
        public static void ForceSuitPlayerSelector(bool full, bool complete, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Force Suit";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                playerFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 

            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            
            allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

            
            foreach (var player in allPlayers)
            {
                if(!player.isPlayerControlled)continue;
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{player.playerUsername}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    ForceSuitSuitSelector(full, complete, player, su);
                });
            }
        } 
        public static void ForceSuitSuitSelector(bool full, bool complete,PlayerControllerB players, bool su = false )
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Force Suit";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                playerFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 

            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            
            allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

            
            
            List<UnlockableItem> items = StartOfRound.Instance.unlockablesList.unlockables;

            foreach (var suit in Networker.orderedSuits)
            {
                string SuitName = items[suit.syncedSuitID.Value].unlockableName;
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                
                buttonText.text = $"{SuitName}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Networker.Instance.suitStuffServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,players), suit.syncedSuitID.Value);
                });
            }
            GameObject effectObj2 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
            
            buttonText2.text = $"Refresh Suits";
            buttonText2.color = Color.red;
            
            Button button2 = effectObj2.GetComponent<Button>();
            button2.onClick.AddListener(() =>
            {
                Networker.orderedSuits = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>()
                    .OrderBy(suit => 
                        suit.syncedSuitID.Value >= 0 &&
                        suit.syncedSuitID.Value < StartOfRound.Instance.unlockablesList.unlockables.Count
                            ? StartOfRound.Instance.unlockablesList.unlockables[suit.syncedSuitID.Value].unlockableName
                            : string.Empty) // Default case to prevent errors
                    .ToList();
                CloseSelectMenu();
                ForceSuitSuitSelector(full, complete, players, su);
            });
        }

        public static Dictionary<string,bool> currentStuff = new Dictionary<string, bool>();
        public static void RefreshRevives()
        {
            currentStuff.TryGetValue("full", out var full);
            currentStuff.TryGetValue("complete", out var complete);
            currentStuff.TryGetValue("su", out var su);
            RevivePlayer(full, complete, su);
        }
        public static void RevivePlayer(bool full, bool complete, bool su = false)
        {
            ReviveOpen = true;
            currentStuff = new Dictionary<string, bool>();
            currentStuff.Add("full",full);
            currentStuff.Add("complete",complete);
            currentStuff.Add("su",su);
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                ReviveOpen = false;
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);
            TMP_Text Panel = EffectMenu.transform.Find("Panel/Text (TMP)").GetComponent<TMP_Text>();
            Panel.text = "Revive Player";

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                playerFunctions(full, complete, su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 

            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            
            allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

            int count = 0;
            
            foreach (var player in allPlayers)
            {
                if(!player.isPlayerDead)continue;
                count++;
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{player.playerUsername}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (MysteryDice.DebugMenuClosesAfter.Value)
                    {
                        CloseSelectMenu();
                    }
                    else
                    {
                        CloseSelectMenu();
                        RevivePlayer(full, complete, su);
                    }
                    Networker.Instance.RevivePlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), StartOfRound.Instance.middleOfShipNode.position);
                    string txtToSay = "";
                    txtToSay = $"Revived {player.playerUsername}"; 
                    Misc.SafeTipMessage($"Revival", txtToSay);
                });
                
            }
            if (count == 0)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"No Dead Players";
            }
        }
        public static void ShowSelectMenu(bool full, bool complete = false, bool su = false, bool fromSaint = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            if(fromSaint) exitText.text = "Exit";
            if (fromSaint)
            {
                exitButton.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                });
            }
            else
            {
                exitButton.onClick.AddListener(() =>
                {
                    showDebugMenu(full, complete ,su);
                });
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            List<IEffect> effects = getOrdered(full, complete);

            
            foreach (IEffect effect in effects)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                
                FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
                List<string> favoriteEffectNames = favoritesData.Favorites; 

                bool isFavorite = favoriteEffectNames.Contains(effect.Name);
                string favoriteMarker = isFavorite ? "*" : "";

                buttonText.text = $"{favoriteMarker} {effect.Name} [{effect.Outcome}] {favoriteMarker}";
                buttonText.color = isFavorite ? Color.gray : Color.black;
                buttonText.outlineColor = Color.black;
                buttonText.outlineWidth = 1;

                if (buttonText.text.Length > 20) 
                {
                    buttonText.fontSize = 12;
                }
                else
                {
                    buttonText.fontSize = 16; 
                }
                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (fromSaint)
                    {
                        CloseSelectMenu();
                    }
                    else
                    {
                        if(MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();
                    }
                    effect.Use();
                });
                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effectName = effect.Name;  
                rightClickHandler.category = "Favorites";    
                rightClickHandler.full = full;
                rightClickHandler.complete = complete;
            }
        } 
        public static void ShowSoundMenu(bool full, bool complete = false, bool su = false)
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }

            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            TMP_Text exitText = EffectMenu.transform.Find("Panel/Exit/Text (TMP)").GetComponent<TMP_Text>();
            exitText.text = "Back";
            exitButton.onClick.AddListener(() =>
            {
                specialFunctions(full, complete ,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            foreach (var entry in MysteryDice.sounds.OrderBy(x=>x.Key))
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

                buttonText.text = entry.Key;
                buttonText.outlineColor = Color.black;
                buttonText.outlineWidth = 1;

                if (buttonText.text.Length > 20) 
                {
                    buttonText.fontSize = 12;
                }
                else
                {
                    buttonText.fontSize = 16; 
                }
                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if(MysteryDice.DebugMenuClosesAfter.Value) CloseSelectMenu();
                    Networker.Instance.PlaySoundServerRPC(entry.Key);
                });
            }
        }

        public static void ToggleFavorite(IEffect effect)
        {
            // Load full favorite data object
            FavoriteEffectManager.FavoriteData favorites = FavoriteEffectManager.LoadFavorites();

            if (favorites.Favorites.Contains(effect.Name))
            {
                favorites.Favorites.Remove(effect.Name);
            }
            else
            {
                favorites.Favorites.Add(effect.Name);
            }

            // Save the updated favorite data
            FavoriteEffectManager.SaveFavorites(favorites);
        }

        public static void CloseSelectMenu()
        {
            if (EffectMenu != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Destroy(EffectMenu);
                currentStuff = new Dictionary<string, bool>();
                ReviveOpen = false;
            }
                
        }
    }
    
    
    public class RightClickHandler : MonoBehaviour, IPointerClickHandler
    {
    public string effectName;
    public string category;
    public bool full;
    public bool complete;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var favorites = FavoriteEffectManager.LoadFavorites();

            switch (category)
            {
                case "Favorites":  
                    ToggleFavorite(effectName, favorites.Favorites);
                    break;
                case "FavoriteEnemies":
                    ToggleFavorite(effectName, favorites.FavoriteEnemies);
                    break;
                case "FavoriteTraps":
                    ToggleFavorite(effectName, favorites.FavoriteTraps);
                    break;
                case "FavoriteScraps":
                    ToggleFavorite(effectName, favorites.FavoriteScraps);
                    break;
                case "FavoriteShopItems":
                    ToggleFavorite(effectName, favorites.FavoriteShopItems);
                    break;
            }

            FavoriteEffectManager.SaveFavorites(favorites);
            SelectEffect.CloseSelectMenu();

         
            switch (category)
            {
                case "Favorites":
                    SelectEffect.ShowSelectMenu(full, complete);
                    break;
                case "FavoriteEnemies":
                    SelectEffect.spawnEnemy(full, complete);
                    break;
                case "FavoriteTraps":
                    SelectEffect.spawnTrap(full, complete);
                    break;
                case "FavoriteScraps":
                    SelectEffect.spawnScrap(full, complete);
                    break;
                case "FavoriteShopItems":
                    SelectEffect.spawnShopItems(full, complete);
                    break;
            }
        }
    }


        private void ToggleFavorite(string name, List<string> favorites)
        {
            if (favorites.Contains(name))
            {
                favorites.Remove(name);
            }
            else
            {
                favorites.Add(name);
            }
        }
    }
    public static class FavoriteEffectManager
    {
        private static readonly string directoryPath = Path.Combine(Application.persistentDataPath, "EmergencyDice");

        private static readonly string filePath = Path.Combine(directoryPath, "Favorites.json");

        public static FavoriteData LoadFavorites()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Favorites file not found at {filePath}. Returning empty data.");
                return new FavoriteData();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<FavoriteData>(json) ?? new FavoriteData();
            }
            catch
            {
                Debug.LogError($"Failed to load favorites from {filePath}. Returning empty data.");
                return new FavoriteData();
            }
        }

        public static void SaveFavorites(FavoriteData favorites)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                string json = JsonConvert.SerializeObject(favorites, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Debug.Log($"Favorites successfully saved to {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save favorites to {filePath}. Error: {ex.Message}");
            }
        }

        public class FavoriteData
        {
            public List<string> Favorites { get; set; } = new List<string>();
            public List<string> FavoriteEnemies { get; set; } = new List<string>();
            public List<string> FavoriteTraps { get; set; } = new List<string>();
            public List<string> FavoriteScraps { get; set; } = new List<string>();
            public List<string> FavoriteShopItems { get; set; } = new List<string>();
            public List<string> FavoriteItems { get; set; } = new List<string>();
        }
    }
}
