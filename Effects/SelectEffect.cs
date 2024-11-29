using GameNetcodeStuff;
using MysteryDice.Dice;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

namespace MysteryDice.Effects
{
    internal class SelectEffect : IEffect
    {
        public string Name => "Select Effect";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can select an effect";

        public static GameObject EffectMenu = null;

        public void Use()
        {
            ShowSelectMenu(false,false);
        }

        public static List<IEffect> getOrdered(bool full, bool complete=false)
        {
            List<IEffect> effects = new List<IEffect>();
            List<IEffect> allEffects = new List<IEffect>(complete ? DieBehaviour.CompleteEffects : (full ? DieBehaviour.AllEffects : DieBehaviour.AllowedEffects));
            List<IEffect> toRemove = new List<IEffect>();
            foreach (var ef in allEffects.Where(x => x.Name.StartsWith("$")))
            {
                effects.Add(ef);
                toRemove.Add(ef);
            }
            foreach (var ef in toRemove)
            {
                allEffects.Remove(ef);
            }
            foreach (var e in DieBehaviour.favConfigs.Where(x => x.Value).OrderBy(x => x.Definition.Key))
            {
                IEffect effect = allEffects.FirstOrDefault(x => x.Name == e.Definition.Key);
                if (effect != null)
                {
                    effects.Add(effect);
                    allEffects.Remove(effect);
                }
            }
            effects.AddRange(allEffects);
            return effects;
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
            if (StartOfRound.Instance.localPlayerController.playerSteamId == 76561198077184650 || StartOfRound.Instance.localPlayerController.playerSteamId == 76561199094139351)
            {
                GameObject effectObj8 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText8.text = $"Play Sound";

                Button button8 = effectObj8.GetComponent<Button>();
                button8.onClick.AddListener(() =>
                {
                    CloseSelectMenu(); 
                    ShowSoundMenu(full, complete,su);
                });
            }
           

            // GameObject effectObj6 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
            // TMP_Text buttonText6 = effectObj6.transform.GetChild(0).GetComponent<TMP_Text>();
            // buttonText6.text = $"Spawn Outside Objects";
            //
            // Button button6 = effectObj6.GetComponent<Button>();
            // button6.onClick.AddListener(() =>
            // {
            //     CloseSelectMenu();
            //     spawnWorldObject(full, complete, su);
            // });
            if (su)
            {
                GameObject effectObj7 = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText7 = effectObj7.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText7.text = $"Grant/Revoke Admin";

                Button button7 = effectObj7.GetComponent<Button>();
                button7.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    grantAdmin(full, complete, su);
                });
            }
           
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
                showDebugMenu(full, complete,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            List<SpawnableEnemyWithRarity> allenemies = new List<SpawnableEnemyWithRarity>();

            foreach (var level in StartOfRound.Instance.levels)
            {
                allenemies = allenemies
                    .Union(level.Enemies)
                    .Union(level.OutsideEnemies)
                    .Union(level.DaytimeEnemies)
                    .ToList();
            }
            allenemies = allenemies
            .GroupBy(x => x.enemyType.enemyName)
            .Select(g => g.First())
            .OrderBy(x => x.enemyType.enemyName)
            .ToList();

            foreach (var enemy in allenemies)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{enemy.enemyType.enemyName}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    Networker.Instance.CustomMonsterServerRPC(enemy.enemyType.enemyName, 1, StartOfRound.Instance.localPlayerController.isInsideFactory);
                    string txtToSay = "";
                    string outsideInside = StartOfRound.Instance.localPlayerController.isInsideFactory ? "Inside" : "Outside";
                    txtToSay = $"Spawned {enemy.enemyType.enemyName} {outsideInside}";
                    Misc.SafeTipMessage($"Spawned {enemy.enemyType.enemyName}", txtToSay);
                });
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
                showDebugMenu(full,complete,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            List<SpawnableMapObject> alltraps = new List<SpawnableMapObject>();

            foreach (var level in StartOfRound.Instance.levels)
            {
                alltraps = alltraps
                    .Union(level.spawnableMapObjects)
                    .ToList();
            }
            alltraps = alltraps
            .GroupBy(x => x.prefabToSpawn.name)
            .Select(g => g.First())
            .OrderBy(x => x.prefabToSpawn.name)
            .ToList();

            foreach (var trap in alltraps)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{trap.prefabToSpawn.name}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    Networker.Instance.spawnTrapOnServerRPC(trap.prefabToSpawn.name, 1, StartOfRound.Instance.localPlayerController.isInsideFactory, StartOfRound.Instance.localPlayerController.playerClientId);
                    string txtToSay = "";
                    txtToSay = $"Spawned {trap.prefabToSpawn.name}";
                    Misc.SafeTipMessage($"Spawned Trap", txtToSay);
                });
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
                showDebugMenu(full, complete,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            List<SpawnableItemWithRarity> allscraps = new List<SpawnableItemWithRarity>(RoundManager.Instance.currentLevel.spawnableScrap);

            foreach (var level in StartOfRound.Instance.levels)
            {
                allscraps = allscraps
                    .Union(level.spawnableScrap)
                    .ToList();
            }
            allscraps = allscraps
            .GroupBy(x => x.spawnableItem.itemName)
            .Select(g => g.First())
            .OrderBy(x => x.spawnableItem.name)
            .ToList();

            foreach (var scrap in allscraps)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{scrap.spawnableItem.name}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    Networker.Instance.SameScrapServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, 1, scrap.spawnableItem.itemName);
                    string txtToSay = "";
                    txtToSay = $"Spawned {scrap.spawnableItem.name}";
                    Misc.SafeTipMessage($"Item Spawned", txtToSay);
                });
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
                    CloseSelectMenu();
                    Networker.Instance.SpawnObjectServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, 1, scrap.spawnableObject.name);
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
                showDebugMenu(full, complete,su);
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; 
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();

            List<Item> allscraps = terminal.buyableItemsList.ToList();

            allscraps = allscraps
            .GroupBy(x => x.itemName)
            .Select(g => g.First())
            .OrderBy(x => x.itemName)
            .ToList();

            foreach (var scrap in allscraps)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{scrap.itemName}";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    Networker.Instance.spawnStoreItemServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId, scrap.itemName);
                    string txtToSay = "";
                    txtToSay = $"Spawned {scrap.itemName}";
                    Misc.SafeTipMessage($"Item Spawned", txtToSay);
                });
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
                    Networker.Instance.becomeAdminServerRPC(player.playerClientId);
                    string txtToSay = "";
                    txtToSay = $"Made {player.playerUsername} an admin"; 
                    Networker.Instance.MessageToHostServerRPC($"Admin", txtToSay);
                });
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

                bool isFavorite = DieBehaviour.favConfigs.Any(x => x.Definition.Key == effect.Name && x.Value);
                string favoriteMarker = isFavorite ? "*" : "";
                buttonText.text = $"{favoriteMarker} {favoriteMarker} {effect.Name} [{effect.Outcome}] {favoriteMarker} {favoriteMarker}";
                if(isFavorite)buttonText.color = Color.gray;else buttonText.color=Color.black;
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
                    CloseSelectMenu();
                    effect.Use();
                });
                RightClickHandler rightClickHandler = effectObj.AddComponent<RightClickHandler>();
                rightClickHandler.effect = effect;
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
                showDebugMenu(full, complete ,su);
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
                    CloseSelectMenu();
                    Networker.Instance.PlaySoundServerRPC(entry.Key);
                });
            }
        }

        public static void ToggleFavorite(IEffect effect)
        {
            var favConfigEntry = DieBehaviour.favConfigs.FirstOrDefault(x => x.Definition.Key == effect.Name);
            if (favConfigEntry != null)
            {
                favConfigEntry.Value = !favConfigEntry.Value; // Toggle the favorite status
            }
        }

        public static void CloseSelectMenu()
        {
            if (EffectMenu != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Destroy(EffectMenu);
            }
                
        }
    }
    //Yay stack overflow!
    public class RightClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public IEffect effect; 
        public bool full;
        public bool complete;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                SelectEffect.ToggleFavorite(effect);
                SelectEffect.CloseSelectMenu();
                SelectEffect.ShowSelectMenu(full,complete);
            }
        }
    }
}
