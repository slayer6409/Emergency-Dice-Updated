using System;
using GameNetcodeStuff;
using MysteryDice.Dice;
using System.Collections.Generic;
using System.Linq;
using LethalLib.Modules;
using MysteryDice.Extensions;
using MysteryDice.Patches;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects;

public class DebugMenuStuff : MonoBehaviour
{
    public static Color ButtonColor = new Color(0.6430231f, 0.2783019f, 1, 1);
    public static Color TextColor = new Color(0.9607844f, 0.5058824f, 0.9803922f, 1);
    public static Color FavoriteTextColor = new Color(0.9607844f, 0.2058824f, 0.2803922f, 1);
    public static Color BackgroundColor = new Color(0.6039216f, 0.2392157f, 1f, 0.2f);
    public static Color AccentColor = new Color(0.6078432f, 0.2588235f, 0.9921569f, 0.4627451f);
    public static bool full, complete, su, ReviveOpen;
    public static GameObject EffectMenu = null!;

    public static TMP_InputField searchInput;
    public static List<Image> backgroundImages = new List<Image>();
    public static List<Image> accentImages = new List<Image>();
    public static List<Image> buttonImages = new List<Image>();
    public static List<TMP_Text> textElements = new List<TMP_Text>();
    public static List<Text> textElements2 = new List<Text>();
    public static bool ran;
    public static bool fromYippee;
    public static bool fromMe;
    public static bool fromRandom;
    public static Transform mainScrollContent;
    public static Transform subScrollContent;
    public static event Action OnDebugMenuOpen;
    public static event Action<Transform> OnSpecialFunctionsAdded;
    public static event Action<Transform> OnSpawnFunctionsAdded;
    public static event Action<Transform> OnPlayerFunctionsAdded;
    
    public static void SetupColors()
    {
        if (ColorUtility.TryParseHtmlString(
                AppendTransparency(MysteryDice.DebugButtonColor.Value, MysteryDice.DebugButtonAlpha.Value),
                out ButtonColor)) ;
        else ColorUtility.TryParseHtmlString("#A447FF", out ButtonColor);
        
        if (ColorUtility.TryParseHtmlString(AppendTransparency(MysteryDice.DebugMenuTextColor.Value, MysteryDice.DebugMenuTextAlpha.Value), out TextColor)) { }
        else ColorUtility.TryParseHtmlString("#F581FA", out TextColor);
        
        if (ColorUtility.TryParseHtmlString(AppendTransparency(MysteryDice.DebugMenuFavoriteTextColor.Value, MysteryDice.DebugMenuFavoriteTextAlpha.Value), out FavoriteTextColor)) { }
        else ColorUtility.TryParseHtmlString("#F53548", out FavoriteTextColor);
        
        if (ColorUtility.TryParseHtmlString(AppendTransparency(MysteryDice.DebugMenuAccentColor.Value, MysteryDice.DebugMenuAccentAlpha.Value), out AccentColor)) { }
        else ColorUtility.TryParseHtmlString("#A34EFF41", out AccentColor);
        
        if (ColorUtility.TryParseHtmlString(AppendTransparency(MysteryDice.DebugMenuBackgroundColor.Value, MysteryDice.DebugMenuBackgroundAlpha.Value), out BackgroundColor)) { }
        else ColorUtility.TryParseHtmlString("#9B42FD76", out BackgroundColor);
    }
    public static void PrintHierarchy(Transform parent, string indent = "")
    {
        Debug.Log(indent + parent.name);
        foreach (Transform child in parent)
        {
            PrintHierarchy(child, indent + "  ");
        }
    }
    public static string AppendTransparency(string hexColor, int percentage)
    {
        // Convert percentage to a value from 0 to 255
        int alpha = (int)Math.Round((percentage / 100.0) * 255);
        
        // Convert the alpha value to a two-digit hexadecimal string
        string alphaHex = alpha.ToString("X2");
        
        // Append the alpha hex to the color string and return
        return hexColor + alphaHex;
    }
    
    public static void setupElements()
    {
        //PrintHierarchy(MysteryDice.DebugMenuPrefab.transform);
        buttonImages.Add(MysteryDice.DebugSubButtonPrefab.GetComponent<Image>());
        buttonImages.Add(MysteryDice.DebugMenuButtonPrefab.GetComponent<Image>());
        textElements.Add(MysteryDice.DebugSubButtonPrefab.GetComponentInChildren<TMP_Text>());
        textElements.Add(MysteryDice.DebugMenuButtonPrefab.GetComponentInChildren<TMP_Text>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/Scroll View/Scrollbar Vertical").GetComponent<Image>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/ClearButton").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/TopPart").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/Image").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background").GetComponent<Image>());
        accentImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/Border").GetComponent<Image>());
        accentImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/Scroll View").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/TopPart/Border/Title").GetComponent<TMP_Text>());
        accentImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/TopPart/Border").GetComponent<Image>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Select Effect").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Select Effect/Text (TMP)").GetComponent<TMP_Text>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/ClearButton/Text (TMP)").GetComponent<TMP_Text>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Spawn Menu").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Spawn Menu/Text (TMP)").GetComponent<TMP_Text>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Player Functions").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Player Functions/Text (TMP)").GetComponent<TMP_Text>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Special Functions").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Special Functions/Text (TMP)").GetComponent<TMP_Text>());
        buttonImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Grant Admin").GetComponent<Image>());
        textElements.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Grant Admin/Text (TMP)").GetComponent<TMP_Text>());
        accentImages.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Scroll View").GetComponent<Image>());
        textElements2.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Toggle/Label").GetComponent<Text>());
        textElements2.Add(MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Bald/Label").GetComponent<Text>());
        setupSelectElements();
        ran = true;
    }
    public static void setupSelectElements()
    {
        //PrintHierarchy(MysteryDice.NewSelectMenuPrefab.transform);
        buttonImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/Scroll View/Scrollbar Vertical").GetComponent<Image>());
        buttonImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/ClearButton").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/TopPart").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/Image").GetComponent<Image>());
        backgroundImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background").GetComponent<Image>());
        accentImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/Border").GetComponent<Image>());
        accentImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/Scroll View").GetComponent<Image>());
        textElements.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/TopPart/Border/Title").GetComponent<TMP_Text>());
        accentImages.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/TopPart/Border").GetComponent<Image>());
        textElements.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/ClearButton/Text (TMP)").GetComponent<TMP_Text>());
        textElements2.Add(MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Bald/Label").GetComponent<Text>());
    }


    public static void setColors()
    {
        var colorBlock = MysteryDice.DebugMenuPrefab.transform.Find("DebugMenu/Background/Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>().colors;
        var colorBlock2 = MysteryDice.NewSelectMenuPrefab.transform.Find("DebugMenu/Background/Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>().colors;
        colorBlock.normalColor = TextColor;
        colorBlock.highlightedColor = TextColor;
        colorBlock.pressedColor = TextColor;
        colorBlock.selectedColor = TextColor;
        colorBlock2.normalColor = TextColor;
        colorBlock2.highlightedColor = TextColor;
        colorBlock2.pressedColor = TextColor;
        colorBlock2.selectedColor = TextColor;

        foreach (var b in buttonImages)
        {
            b.color = ButtonColor;
        }
        foreach (var text in textElements)
        {
            text.color = TextColor;
        }
        foreach (var text2 in textElements2)
        {
            text2.color = TextColor;
        }
        foreach (var accent in accentImages)
        {
            accent.color = AccentColor;
        }
        foreach (var background in backgroundImages)
        {
            background.color = BackgroundColor;
        }
        
        
    }
    public static void showDebugMenu(bool fulls, bool completes, bool sus = false)
    {   
        
        SetupColors();
        if(!ran) setupElements();
        if(ran) setColors();
        if (EffectMenu != null)
        {
            GameObject.Destroy(EffectMenu);
            EffectMenu = null;
        }
        OnDebugMenuOpen?.Invoke();
        EffectMenu = GameObject.Instantiate(MysteryDice.DebugMenuPrefab);
        if(MysteryDice.LockDebugUI.Value) StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = true;
        subScrollContent = EffectMenu.transform.Find("DebugMenu/Scroll View/Viewport/Content");
        mainScrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");
        InputAction escAction = new InputAction(binding: "<Keyboard>/escape");
        escAction.performed += ctx => CloseSelectMenu();
        escAction.Enable();
        var localPlayer = StartOfRound.Instance.localPlayerController;
        searchInput = EffectMenu.transform.Find("DebugMenu/Background/SearchField").GetComponent<TMP_InputField>();
        searchInput.onValueChanged.AddListener(FilterItems);
        Button ClearButton = EffectMenu.transform.Find("DebugMenu/Background/ClearButton").GetComponent<Button>();
        ClearButton.onClick.AddListener(() =>
        {
            searchInput.text = "";
        });
        full = fulls;
        complete = completes;
        su = sus;

        if (!SelectEffect.isSpecial())
        {
            if (!MysteryDice.superDebugMode.Value && !StartOfRound.Instance.IsHost)
            {
                full = false;
                complete = false;
                su = false;
            }
        }
        if (EffectMenu == null)
        {
            Debug.LogError("EffectMenu is null after instantiation!");
            return;
        }
        DebugMenuController controller = EffectMenu.AddComponent<DebugMenuController>();
        controller.EffectMenu = EffectMenu;
        
        fromRandom = fromMe = fromYippee = false;
        
        if (StartOfRound.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID)
        {
            EffectMenu.transform.Find("DebugMenu/Me").gameObject.SetActive(true);
            EffectMenu.transform.Find("DebugMenu/Yippee").gameObject.SetActive(true);
            EffectMenu.transform.Find("DebugMenu/Random").gameObject.SetActive(true);
            Toggle meChanged = EffectMenu.transform.Find("DebugMenu/Me").GetComponent<Toggle>();
            Toggle yippeeChanged = EffectMenu.transform.Find("DebugMenu/Yippee").GetComponent<Toggle>();
            Toggle randomChanged = EffectMenu.transform.Find("DebugMenu/Random").GetComponent<Toggle>();
            meChanged.onValueChanged.AddListener((bool isOn) =>
            {
                fromMe = isOn;
                if (isOn) fromYippee = fromRandom = false; 
            });

            yippeeChanged.onValueChanged.AddListener((bool isOn) =>
            {
                fromYippee = isOn;
                if (isOn) fromMe = fromRandom = false;
            });

            randomChanged.onValueChanged.AddListener((bool isOn) =>
            {
                fromRandom = isOn;
                if (isOn) fromMe = fromYippee = false;
            });

        }
        Toggle valueChanged2 = EffectMenu.transform.Find("DebugMenu/Bald").GetComponent<Toggle>();
        valueChanged2.isOn = MysteryDice.Bald.Value;
        GameObject BaldImage = EffectMenu.transform.Find("DebugMenu/Background/Image").gameObject;
        valueChanged2.onValueChanged.AddListener((bool isOn) =>
        {
            MysteryDice.Bald.Value = isOn;
            BaldImage.SetActive(isOn);
        });
        if (MysteryDice.Bald.Value)
        {
            BaldImage.SetActive(true);
        }
        else
        {
            BaldImage.SetActive(false);
        }
        
        
      
        Toggle valueChanged = EffectMenu.transform.Find("DebugMenu/Toggle").GetComponent<Toggle>();
        valueChanged.isOn = !MysteryDice.DebugMenuClosesAfter.Value;
        valueChanged.onValueChanged.AddListener((bool isOn) =>
        {
            MysteryDice.DebugMenuClosesAfter.Value = !isOn;
        });
        
        Toggle moveValue = EffectMenu.transform.Find("DebugMenu/Move").GetComponent<Toggle>();
        moveValue.isOn = !MysteryDice.LockDebugUI.Value;
        moveValue.onValueChanged.AddListener((bool isOn) =>
        {
            MysteryDice.LockDebugUI.Value = !isOn;
            StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = MysteryDice.LockDebugUI.Value;
        });

        Button exitButton = EffectMenu.transform.Find("DebugMenu/CloseButton").GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            CloseSelectMenu(true);
        });
        Button selectEffectButton = EffectMenu.transform.Find("DebugMenu/Select Effect").GetComponent<Button>();
        TMP_Text selectEffectText =
            EffectMenu.transform.Find("DebugMenu/Select Effect/Text (TMP)").GetComponent<TMP_Text>();
        selectEffectButton.onClick.AddListener(() =>
        {
            clearMainViewport();
            ShowSelectMenu();
            FilterItems(searchInput.text);
        });
        Button spawnMenuButton = EffectMenu.transform.Find("DebugMenu/Spawn Menu").GetComponent<Button>();
        TMP_Text spawnMenuText = EffectMenu.transform.Find("DebugMenu/Spawn Menu/Text (TMP)").GetComponent<TMP_Text>();
        spawnMenuButton.onClick.AddListener(() =>
        {
            clearSubViewport();
            spawnFunctions();
            FilterItems(searchInput.text);
        });
        Button PlayerButton = EffectMenu.transform.Find("DebugMenu/Player Functions").GetComponent<Button>();
        TMP_Text PlayerText = EffectMenu.transform.Find("DebugMenu/Player Functions/Text (TMP)").GetComponent<TMP_Text>();
        PlayerButton.onClick.AddListener(() =>
        {
            clearSubViewport();
            playerFunctions();
            FilterItems(searchInput.text);
        });
        Button SpecialButton = EffectMenu.transform.Find("DebugMenu/Special Functions").GetComponent<Button>();
        SpecialButton.onClick.AddListener(() =>
        {
            clearSubViewport();
            specialFunctions(SelectEffect.isSpecial());
            FilterItems(searchInput.text);
        });
      
        
        
        Button adminButton = EffectMenu.transform.Find("DebugMenu/Grant Admin").GetComponent<Button>();
        if (su || StartOfRound.Instance.localPlayerController.IsHost)
        {
            TMP_Text adminText = EffectMenu.transform.Find("DebugMenu/Grant Admin/Text (TMP)").GetComponent<TMP_Text>();
            adminButton.onClick.AddListener(() =>
            {
                clearSubViewport();
                AdminFunctions();
                FilterItems(searchInput.text);
            });
        }
        else
        {
            Destroy(adminButton.gameObject);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }
    
    public static void ShowSelectEffectMenu(bool isAdMenu = false)
    {   
        SetupColors();
        if(!ran) setupElements();
        if(ran) setColors();
        
        if (EffectMenu != null)
        {
            GameObject.Destroy(EffectMenu);
            EffectMenu = null;
        }
        
        full = false;
        complete = false;
        su = false;
        
        EffectMenu = GameObject.Instantiate(MysteryDice.NewSelectMenuPrefab);
        if(MysteryDice.LockDebugUI.Value)StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = true;
        mainScrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");
        InputAction escAction = new InputAction(binding: "<Keyboard>/escape");
        escAction.performed += ctx => CloseSelectMenu();
        escAction.Enable();

        TMP_InputField searchInput = EffectMenu.transform.Find("DebugMenu/Background/SearchField").GetComponent<TMP_InputField>();
        searchInput.onValueChanged.AddListener(FilterItems);
        Button ClearButton = EffectMenu.transform.Find("DebugMenu/Background/ClearButton").GetComponent<Button>();
        ClearButton.onClick.AddListener(() =>
        {
            searchInput.text = "";
        });
        if (EffectMenu == null)
        {
            Debug.LogError("EffectMenu is null after instantiation!");
            return;
        }
        DebugMenuController controller = EffectMenu.AddComponent<DebugMenuController>();
        controller.EffectMenu = EffectMenu;
        
        Toggle valueChanged2 = EffectMenu.transform.Find("DebugMenu/Bald").GetComponent<Toggle>();
        valueChanged2.isOn = MysteryDice.Bald.Value;
        GameObject BaldImage = EffectMenu.transform.Find("DebugMenu/Background/Image").gameObject;
        valueChanged2.onValueChanged.AddListener((bool isOn) =>
        {
            MysteryDice.Bald.Value = isOn;
            BaldImage.SetActive(isOn);
        });
        if (MysteryDice.Bald.Value)
        {
            BaldImage.SetActive(true);
        }
        else
        {
            BaldImage.SetActive(false);
        }
        
        Button exitButton = EffectMenu.transform.Find("DebugMenu/CloseButton").GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            CloseSelectMenu(true);
        });
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (!isAdMenu) ShowSelectMenu(true);
        else
        {
            EffectMenu.transform.Find("DebugMenu/TopPart/Border/Title").GetComponent<TMP_Text>().text = "Select an Ad";
            EffectMenu.transform.Find("DebugMenu/Background/TopText").gameObject.SetActive(true);
            EffectMenu.transform.Find("DebugMenu/Background/BottomText").gameObject.SetActive(true);
            AdUnlockable(true);
            AdAnyItem(true);
        }
    }

    static void FilterItems(string searchText)
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");
        foreach (Transform child in scrollContent)
        {
            var textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent)
            {
                bool matches = string.IsNullOrEmpty(searchText) || textComponent.text.ToLower().Contains(searchText.ToLower());
                child.gameObject.SetActive(matches);
            }
        }
    }

    public static void clearMainViewport()
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");
        hideStuff();
        foreach (Transform obj in scrollContent)
        {
            Destroy(obj.gameObject);
        }
    }

    public static void hideStuff()
    {
        EffectMenu.transform.Find("DebugMenu/Background/TopText").gameObject.SetActive(false);
        EffectMenu.transform.Find("DebugMenu/Background/BottomText").gameObject.SetActive(false);
        EffectMenu.transform.Find("DebugMenu/Background/SizeModifiers").gameObject.SetActive(false);
    }

    public static void clearSubViewport()
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Scroll View/Viewport/Content");
        foreach (Transform obj in scrollContent)
        {
            Destroy(obj.gameObject);
        }
    }

    public static List<IEffect> getOrdered()
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

    public static void spawnEnemy()
    {
        // Load favorites
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteEnemyNames = favoritesData.FavoriteEnemies;
        
        var allEnemies = GetEnemies.allEnemies.GroupBy(x=>x.enemyName).Select(g=>g.First()).OrderBy(e => favoriteEnemyNames.Contains(e.enemyName) ? 0 : 1)
            .ThenBy(e => e.enemyName)
            .ToList();
        
        foreach (var enemy in allEnemies)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteEnemyNames.Contains(enemy.enemyName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {enemy.enemyName}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();

                if (StartOfRound.Instance.localPlayerController.isPlayerDead )
                {
                    Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyName,
                        StartOfRound.Instance.spectateCamera.transform.position);
                }
                else
                {
                    if (MysteryDice.debugSpawnOnPlayer.Value)
                    {
                        Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyName,
                            StartOfRound.Instance.localPlayerController.transform.position);
                    }
                    else
                    {
                        Networker.Instance.CustomMonsterServerRPC(enemy.enemyName, 1, 1,
                            StartOfRound.Instance.localPlayerController.isInsideFactory);
                    }
                   
                }

                string outsideInside =
                    StartOfRound.Instance.localPlayerController.isInsideFactory ? "Inside" : "Outside";
                string txtToSay = $"Spawned {enemy.enemyName} {outsideInside}";
                Misc.SafeTipMessage($"Spawned {enemy.enemyName}", txtToSay);
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = enemy.enemyName;
            rightClickHandler.category = "FavoriteEnemies";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    } 

    public static void spawnMiniEnemy()
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteEnemyNames = favoritesData.FavoriteEnemies;

        // List<SpawnableEnemyWithRarity> allEnemies = StartOfRound.Instance.levels
        //     .SelectMany(level => level.Enemies.Union(level.OutsideEnemies).Union(level.DaytimeEnemies))
        //     .GroupBy(x => x.enemyType.enemyName)
        //     .Select(g => g.First())
        //     .ToList();
        //
        // allEnemies = allEnemies.OrderBy(e => favoriteEnemyNames.Contains(e.enemyType.enemyName) ? 0 : 1)
        //     .ThenBy(e => e.enemyType.enemyName)
        //     .ToList();

        
        EffectMenu.transform.Find("DebugMenu/Background/SizeModifiers").gameObject.SetActive(true);
         
        var allEnemies = GetEnemies.allEnemies.GroupBy(x=>x.enemyName).Select(g=>g.First()).OrderBy(e => favoriteEnemyNames.Contains(e.enemyName) ? 0 : 1)
            .ThenBy(e => e.enemyName)
            .ToList();
        
        foreach (var enemy in allEnemies)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteEnemyNames.Contains(enemy.enemyName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {enemy.enemyName}";
            buttonText.color = isFavorite ? Color.red : TextColor;

            var sizeMod = EffectMenu.transform.Find("DebugMenu/Background/SizeModifiers").gameObject;
            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                
                var sizex = float.Parse(sizeMod.transform.Find("X").GetComponent<TMP_InputField>().text);
                var sizey = float.Parse(sizeMod.transform.Find("Y").GetComponent<TMP_InputField>().text);
                var sizez = float.Parse(sizeMod.transform.Find("Z").GetComponent<TMP_InputField>().text);

                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyName,
                        StartOfRound.Instance.spectateCamera.transform.position, true,
                        new Vector3(sizex, sizey, sizez));
                }
                else
                {
                    Networker.Instance.SpawnEnemyAtPosServerRPC(enemy.enemyName,
                        StartOfRound.Instance.localPlayerController.transform.position, true,
                        new Vector3(sizex, sizey, sizez));
                }

                string outsideInside =
                    StartOfRound.Instance.localPlayerController.isInsideFactory ? "Inside" : "Outside";
                string txtToSay = $"Spawned {enemy.enemyName} {outsideInside}";
                Misc.SafeTipMessage($"Spawned {enemy.enemyName}", txtToSay);
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = enemy.enemyName;
            rightClickHandler.category = "FavoriteEnemies";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    }  
    public static void spawnFreebirdEnemy()
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteEnemyNames = favoritesData.FavoriteEnemies;

        // List<SpawnableEnemyWithRarity> allEnemies = StartOfRound.Instance.levels
        //     .SelectMany(level => level.Enemies.Union(level.OutsideEnemies).Union(level.DaytimeEnemies))
        //     .GroupBy(x => x.enemyType.enemyName)
        //     .Select(g => g.First())
        //     .ToList();
        //
        // allEnemies = allEnemies.OrderBy(e => favoriteEnemyNames.Contains(e.enemyType.enemyName) ? 0 : 1)
        //     .ThenBy(e => e.enemyType.enemyName)
        //     .ToList();

        
        var allEnemies = GetEnemies.allEnemies.GroupBy(x=>x.enemyName).Select(g=>g.First()).OrderBy(e => favoriteEnemyNames.Contains(e.enemyName) ? 0 : 1)
            .ThenBy(e => e.enemyName)
            .ToList();
        
        foreach (var enemy in allEnemies)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteEnemyNames.Contains(enemy.enemyName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {enemy.enemyName}";
            buttonText.color = isFavorite ? Color.red : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();

                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    Networker.Instance.SpawnFreebirdEnemyServerRPC(enemy.enemyName,true, true, StartOfRound.Instance.spectateCamera.transform.position);
                }
                else
                {
                    var RM = RoundManager.Instance;
                    
                    Vector3 position = Vector3.zero;
            
                    bool usePos = false;
                    if(MysteryDice.debugSpawnOnPlayer.Value)
                    {
                        usePos = true;
                        position = StartOfRound.Instance.localPlayerController.transform.position;
                    }
                    Networker.Instance.SpawnFreebirdEnemyServerRPC(enemy.enemyName, enemy.isDaytimeEnemy||enemy.isOutsideEnemy, usePos ,position);
                }
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = enemy.enemyName;
            rightClickHandler.category = "FavoriteEnemies";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    } 
    public static void spawnFreebirdTrap()
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteTrapNames = favoritesData.FavoriteTraps;

        var allTraps = Misc.getAllTraps()
            .GroupBy(x => x.name)
            .Select(g => g.First())
            .ToList();
        //trap trapToSpawn;

        allTraps = allTraps.OrderBy(t => favoriteTrapNames.Contains(t.name) ? 0 : 1)
            .ThenBy(t => t.name)
            .ToList();

        
        foreach (var trap in allTraps)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteTrapNames.Contains(trap.name);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {trap.name}";
            buttonText.color = isFavorite ? Color.red : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Networker.Instance.SpawnFreebirdTrapServerRPC(trap.name);
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = trap.name;
            rightClickHandler.category = "FavoriteTraps";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    } 

    public static void spawnTrap()
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteTrapNames = favoritesData.FavoriteTraps;
       
        var allTraps = Misc.getAllTraps()
            .GroupBy(x => x.name)
            .Select(g => g.First())
            .ToList();
        //trap trapToSpawn;

        allTraps = allTraps.OrderBy(t => favoriteTrapNames.Contains(t.name) ? 0 : 1)
            .ThenBy(t => t.name)
            .ToList();


        foreach (var trap in allTraps)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteTrapNames.Contains(trap.name);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {trap.name}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();

                Vector3 spawnPosition;
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    spawnPosition = StartOfRound.Instance.spectateCamera.transform.position;
                }
                else
                {
                    spawnPosition =
                        StartOfRound.Instance.localPlayerController.transform.position;
                }

                Networker.Instance.spawnTrapOnServerRPC(trap.name, 1,
                    StartOfRound.Instance.localPlayerController.isInsideFactory,
                    Array.IndexOf(StartOfRound.Instance.allPlayerScripts, StartOfRound.Instance.localPlayerController),
                    StartOfRound.Instance.localPlayerController.isPlayerDead||StartOfRound.Instance.inShipPhase,
                    spawnPosition);
                Misc.SafeTipMessage($"Spawned {trap.name}", $"Spawned at {spawnPosition}");
            });

            // ✅ Right-click handler for favoriting
            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = trap.name;
            rightClickHandler.category = "FavoriteTraps";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    } //

    public static void spawnScrap()
    {
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
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteScrapNames.Contains(scrap.spawnableItem.itemName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {scrap.spawnableItem.itemName}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();

                Vector3 spawnPosition;
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    spawnPosition = StartOfRound.Instance.spectateCamera.transform.position; // ✅ Spectator Fix
                }
                else
                {
                    spawnPosition =
                        StartOfRound.Instance.localPlayerController.transform.position; // ✅ Normal Player Spawn
                }

                Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, GameNetworkManager.Instance.localPlayerController),
                    1, scrap.spawnableItem.itemName, StartOfRound.Instance.localPlayerController.isPlayerDead,
                    spawnPosition);
                Misc.SafeTipMessage($"Spawned {scrap.spawnableItem.name}", $"Spawned at {spawnPosition}");
            });

            // ✅ Right-click handler for favoriting
            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = scrap.spawnableItem.itemName;
            rightClickHandler.category = "FavoriteScraps";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    }
    public static void spawnAnyItem()
    {

        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteItemsNames = favoritesData.FavoriteItems;

        var allScraps = StartOfRound.Instance.allItemsList.itemsList.ToList().OrderBy(s => favoriteItemsNames.Contains(s.itemName) ? 0 : 1)
            .ThenBy(s => s.itemName)
            .ToList();
        
        var localPlayer = StartOfRound.Instance.localPlayerController;
        foreach (Item scrap in allScraps)
        {
            GameObject prefab = scrap.spawnPrefab;
            int prefabIndex = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs
                .FindIndex(p => p.Prefab == prefab);
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteItemsNames.Contains(scrap.itemName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {scrap.itemName}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();

                Vector3 spawnPosition;
                if (localPlayer.isPlayerDead)
                {
                    spawnPosition = StartOfRound.Instance.spectateCamera.transform.position;
                }
                else
                {
                    spawnPosition =
                        localPlayer.transform.position; 
                }
                int count = Keyboard.current.leftShiftKey.isPressed ? 5 : 1;
                Networker.Instance.SameScrapAdvancedServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, GameNetworkManager.Instance.localPlayerController),
                    count, scrap.itemName, localPlayer.isPlayerDead,
                    spawnPosition, networkPrefabIndex: prefabIndex);
                Misc.SafeTipMessage($"Spawned {scrap.name}", $"Spawned at {spawnPosition}");
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = scrap.itemName;
            rightClickHandler.category = "FavoriteItems";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    } 
    
    
    public static void AdAnyItem(bool fromSelect = false)
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteItemsNames = favoritesData.FavoriteItems;

        var allScraps = StartOfRound.Instance.allItemsList.itemsList.ToList().OrderBy(s => favoriteItemsNames.Contains(s.itemName) ? 0 : 1)
            .ThenBy(s => s.itemName)
            .ToList();
        foreach (Item scrap in allScraps)
        {
            
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteItemsNames.Contains(scrap.itemName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {scrap.itemName}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu(fromSelect);
                Networker.Instance.AdServerRPC(true, scrap.itemName, getTopText(), getBottomText());
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = scrap.itemName;
            rightClickHandler.category = "FavoriteAdItems";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
        }
    }

    public static string getTopText()
    {
        string toReturn = ManyAds.getTopText();
        var TopText = EffectMenu.transform.Find("DebugMenu/Background/TopText").GetComponent<TMP_InputField>();
        if(TopText.text!="") toReturn = TopText.text;
        return toReturn;
    }

    public static string getBottomText()
    {
        string toReturn = ManyAds.getBottomText();
        var BottomText = EffectMenu.transform.Find("DebugMenu/Background/BottomText").GetComponent<TMP_InputField>();
        if(BottomText.text!="") toReturn = BottomText.text;
        return toReturn;
    }
    
    public static void AdUnlockable(bool fromSelect = false)
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        foreach (var item in StartOfRound.Instance.unlockablesList.unlockables)
        {
            if(item.prefabObject==null)continue;
            
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            buttonText.text = item.unlockableName;
            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Networker.Instance.AdServerRPC(false, item.unlockableName, getTopText(), getBottomText());
                CloseSelectMenu(fromSelect);
            });
        }
    }
    

    public static void spawnWorldObject()
    {
        
        List<SpawnableOutsideObject> allObjects = new List<SpawnableOutsideObject>();
        allObjects = GetEnemies.allObjects.ToList();
        allObjects = allObjects.GroupBy(x => x.name)
            .Select(g => g.First())
            .OrderBy(x => x.name)
            .ToList();
        
        foreach (var scrap in allObjects)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{scrap.name}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Networker.Instance.SpawnObjectServerRPC(
                    Array.IndexOf(StartOfRound.Instance.allPlayerScripts, GameNetworkManager.Instance.localPlayerController), 1, scrap.name);
                string txtToSay = "";
                txtToSay = $"Spawned {scrap.name}";
                Misc.SafeTipMessage($"Object Spawned", txtToSay);
            });
        }
    }
    
    public static void spawnShopItems()
    {
        FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
        List<string> favoriteShopItemNames = favoritesData.FavoriteShopItems;

        Terminal terminal = GameObject.FindObjectOfType<Terminal>();
        List<Item> allShopItems = terminal.buyableItemsList
            .GroupBy(x => x.itemName)
            .Select(g => g.First())
            .ToList();

        allShopItems = allShopItems.OrderBy(s => favoriteShopItemNames.Contains(s.itemName) ? 0 : 1)
            .ThenBy(s => s.itemName)
            .ToList();

        foreach (var item in allShopItems)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            bool isFavorite = favoriteShopItemNames.Contains(item.itemName);
            string favoriteMarker = isFavorite ? "*" : "";
            buttonText.text = $"{favoriteMarker} {item.itemName}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Vector3 spawnPosition;
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    spawnPosition = StartOfRound.Instance.spectateCamera.transform.position;
                }
                else
                {
                    spawnPosition = StartOfRound.Instance.localPlayerController.transform.position;
                }

                Networker.Instance.spawnStoreItemServerRPC(
                    Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController),
                    item.itemName, spawnPosition);
            });

            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = item.itemName;
            rightClickHandler.category = "FavoriteShopItems";
        }
    }
    public static void grantAdmin(bool grant)
    {
        List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();

        allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();

        foreach (var player in allPlayers)
        {
            if (!Misc.IsPlayerReal(player)) continue;
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{player.playerUsername}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Networker.Instance.becomeAdminServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), grant);
                string txtToSay = "";
                txtToSay = $"Made {player.playerUsername} an admin"; 
            });
        }
    } 
    public static void spawnFunctions()
    {
        GameObject effectObj2 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText2.text = $"Spawn Enemy";

        Button button2 = effectObj2.GetComponent<Button>();
        button2.onClick.AddListener(() =>
        {
            clearMainViewport();
            spawnEnemy();
            FilterItems(searchInput.text);
        });
        GameObject effectObj7 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText7 = effectObj7.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText7.text = $"Spawn Items (any)";

        Button button7 = effectObj7.GetComponent<Button>();
        button7.onClick.AddListener(() =>
        {
            clearMainViewport();
            spawnAnyItem();
            FilterItems(searchInput.text);
        });
        
        // GameObject effectObj3 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        // TMP_Text buttonText3 = effectObj3.transform.GetChild(0).GetComponent<TMP_Text>();
        // buttonText3.text = $"Spawn Scrap";
        //
        // Button button3 = effectObj3.GetComponent<Button>();
        // button3.onClick.AddListener(() =>
        // {
        //     clearMainViewport();
        //     spawnScrap();
        //     FilterItems(searchInput.text);
        // });

        // GameObject effectObj5 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        // TMP_Text buttonText5 = effectObj5.transform.GetChild(0).GetComponent<TMP_Text>();
        // buttonText5.text = $"Spawn Shop Items";
        //
        // Button button5 = effectObj5.GetComponent<Button>();
        // button5.onClick.AddListener(() =>
        // {
        //     clearMainViewport();
        //     spawnShopItems();
        //     FilterItems(searchInput.text);
        // });

        GameObject effectObj4 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText4 = effectObj4.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText4.text = $"Spawn Trap";

        Button button4 = effectObj4.GetComponent<Button>();
        button4.onClick.AddListener(() =>
        {
            clearMainViewport();
            spawnTrap();
            FilterItems(searchInput.text);
        });

        if (SelectEffect.isSpecial() || StartOfRound.Instance.localPlayerController.IsHost)
        {
            GameObject effectObj6 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
            TMP_Text buttonText6 = effectObj6.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText6.text = $"Spawn Outside Objects";

            Button button6 = effectObj6.GetComponent<Button>();
            button6.onClick.AddListener(() =>
            {
                clearMainViewport();
                spawnWorldObject();
                FilterItems(searchInput.text);
            });
            
            GameObject effectObj9 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
            TMP_Text buttonText9 = effectObj9.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText9.text = $"Spawn Sized Enemy";

            Button button9 = effectObj9.GetComponent<Button>();
            button9.onClick.AddListener(() =>
            {
                clearMainViewport();
                spawnMiniEnemy();
                FilterItems(searchInput.text);
            });
            
            if (StartOfRound.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID || StartOfRound.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.disableSteam)
            {
                GameObject effectObj8 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
                TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText8.text = $"Spawn Freebird Enemy";

                Button button8 = effectObj8.GetComponent<Button>();
                button8.onClick.AddListener(() =>
                {
                    clearMainViewport();
                    spawnFreebirdEnemy();
                    FilterItems(searchInput.text);
                });
                GameObject effectObj10 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
                TMP_Text buttonText10 = effectObj10.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText10.text = $"Spawn Freebird Trap";

                Button button10 = effectObj10.GetComponent<Button>();
                button10.onClick.AddListener(() =>
                {
                    clearMainViewport();
                    spawnFreebirdTrap();
                    FilterItems(searchInput.text);
                });
            }
        }
        OnSpawnFunctionsAdded?.Invoke(subScrollContent);
    }
    public static void specialFunctions(bool special)
    {
        if (special)
        {
            GameObject effectObj8 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
            TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText8.text = $"Play Sound";

            Button button8 = effectObj8.GetComponent<Button>();
            button8.onClick.AddListener(() =>
            {
                clearMainViewport();
                ShowSoundMenu();
                FilterItems(searchInput.text);
            });
            
            GameObject effectObj9 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
            TMP_Text buttonText9 = effectObj9.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText9.text = $"Jumpscare";

            Button button9 = effectObj9.GetComponent<Button>();
            button9.onClick.AddListener(() =>
            {
                clearMainViewport();
                ShowJumpscareMenu();
                FilterItems(searchInput.text);
            });
            

        }

        if (StartOfRound.Instance.localPlayerController.IsHost || special)
        {
            GameObject effectObj1 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
            TMP_Text buttonText1 = effectObj1.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText1.text = $"Unlock Unlockable";

            Button button1 = effectObj1.GetComponent<Button>();
            button1.onClick.AddListener(() =>
            {
                clearMainViewport();
                UnlockSomething();
                FilterItems(searchInput.text);
            });
            
            
        }
        
        
        GameObject effectObj10 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText10 = effectObj10.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText10.text = $"AdItem";

        Button button10 = effectObj10.GetComponent<Button>();
        button10.onClick.AddListener(() =>
        {
            clearMainViewport();
            if (SelectEffect.isSpecial())
            {
                EffectMenu.transform.Find("DebugMenu/Background/TopText").gameObject.SetActive(true);
                EffectMenu.transform.Find("DebugMenu/Background/BottomText").gameObject.SetActive(true);
            }
            AdAnyItem();
            FilterItems(searchInput.text);
        });
            
        GameObject effectObj11 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText11 = effectObj11.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText11.text = $"Ad Unlockable";

        Button button11 = effectObj11.GetComponent<Button>();
        button11.onClick.AddListener(() =>
        {
            clearMainViewport();
            if (SelectEffect.isSpecial())
            {
                EffectMenu.transform.Find("DebugMenu/Background/TopText").gameObject.SetActive(true);
                EffectMenu.transform.Find("DebugMenu/Background/BottomText").gameObject.SetActive(true);
            }
            AdUnlockable();
            FilterItems(searchInput.text);
        });

        GameObject effectObj2 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText2.text = $"POI Teleports";

        Button button2 = effectObj2.GetComponent<Button>();
        button2.onClick.AddListener(() =>
        {
            clearMainViewport();
            poiTeleports();
            FilterItems(searchInput.text);
        });
        
        GameObject effectObj3 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText3 = effectObj3.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText3.text = $"Enemy Teleports";

        Button button3 = effectObj3.GetComponent<Button>();
        button3.onClick.AddListener(() =>
        {
            clearMainViewport();
            enemyTeleports();
            FilterItems(searchInput.text);
        });
        
        GameObject effectObj4 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, subScrollContent);
        TMP_Text buttonText4 = effectObj4.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText4.text = $"Scrap Teleports";

        Button button4 = effectObj4.GetComponent<Button>();
        button4.onClick.AddListener(() =>
        {
            clearMainViewport();
            scrapTeleports();
            FilterItems(searchInput.text);
        });
        
        OnSpecialFunctionsAdded?.Invoke(subScrollContent);
    }
    public static void poiTeleports()
    {
        GameObject effectObj1 = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
        TMP_Text buttonText1 = effectObj1.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText1.text = "Ship";

        Button button1 = effectObj1.GetComponent<Button>();
        button1.onClick.AddListener(() =>
        {
            Networker.Instance.TeleportOrBringPlayerToPosServerRPC(StartOfRound.Instance.middleOfShipNode.position,Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance
                .localPlayerController));
            GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = true;
            GameNetworkManager.Instance.localPlayerController.isInsideFactory = false;
            GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            CloseSelectMenu();
        });
        var entrances = GameObject.FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None);
        foreach (var entrance in entrances)
        {
            GameObject effectObj2 = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText2.text = (entrance.entranceId == 0 ? "Main": "Fire Exit") + " " + (entrance.isEntranceToBuilding ? "Inside":"Outside");

            Button button2 = effectObj2.GetComponent<Button>();
            button2.onClick.AddListener(() =>
            {
                entrance.TeleportPlayer();
                CloseSelectMenu();
            });
        }
    }  
   
    public static void enemyTeleports()
    {
        foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
        {
            GameObject effectObj8 = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText8.text = enemy.enemyType.enemyName;

            Button button8 = effectObj8.GetComponent<Button>();
            button8.onClick.AddListener(() =>
            {
                
                Networker.Instance.TeleportOrBringPlayerToPosServerRPC(enemy.transform.position, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance
                    .localPlayerController));
                CloseSelectMenu();
                GameNetworkManager.Instance.localPlayerController.isInsideFactory = !enemy.isOutside;
                });
        }
    } 
    public static void scrapTeleports()
    {
        var allScrap = GameObject.FindObjectsOfType<GrabbableObject>();
        
        foreach (var scrap in allScrap)
        {
            GameObject effectObj8 = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, mainScrollContent);
            TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText8.text = scrap.name;

            Button button8 = effectObj8.GetComponent<Button>();
            button8.onClick.AddListener(() =>
            {
                
                Networker.Instance.TeleportOrBringPlayerToPosServerRPC(scrap.transform.position, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance
                    .localPlayerController));
                CloseSelectMenu();
                GameNetworkManager.Instance.localPlayerController.isInsideFactory = !(scrap.transform.position.y<-1000); 
                });
        }
    }
    public static void AdminFunctions()
    {

        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Scroll View/Viewport/Content");

        GameObject effectObj8 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
        TMP_Text buttonText8 = effectObj8.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText8.text = $"Grant";

        Button button8 = effectObj8.GetComponent<Button>();
        button8.onClick.AddListener(() =>
        {
            clearMainViewport();
            grantAdmin(true);
            FilterItems(searchInput.text);
        });

        GameObject effectObj9 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
        TMP_Text buttonText9 = effectObj9.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText9.text = $"Revoke";

        Button button9 = effectObj9.GetComponent<Button>();
        button9.onClick.AddListener(() =>
        {
            clearMainViewport();
            grantAdmin(false);
            FilterItems(searchInput.text);
        });
    }
    public static void playerFunctions()
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Scroll View/Viewport/Content");

        GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
        TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText.text = $"Revive Player";

        Button button = effectObj.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            clearMainViewport();
            RevivePlayer();
            FilterItems(searchInput.text);
        });
        GameObject effectObj2 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
        TMP_Text buttonText2 = effectObj2.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText2.text = $"Teleport to Player";

        Button button2 = effectObj2.GetComponent<Button>();
        button2.onClick.AddListener(() =>
        {
            clearMainViewport();
            TeleportPlayer();
            FilterItems(searchInput.text);
        });

        GameObject effectObj3 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
        TMP_Text buttonText3 = effectObj3.transform.GetChild(0).GetComponent<TMP_Text>();
        buttonText3.text = $"Bring Player";

        Button button3 = effectObj3.GetComponent<Button>();
        button3.onClick.AddListener(() =>
        {
            clearMainViewport();
            TeleportPlayer(bring: true);
            FilterItems(searchInput.text);
        });

        if (SelectEffect.isSpecial())
        {
            GameObject effectObj4 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
            TMP_Text buttonText4 = effectObj4.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText4.text = $"Force Suit";

            Button button4 = effectObj4.GetComponent<Button>();
            button4.onClick.AddListener(() =>
            {
                clearMainViewport();
                ForceSuitPlayerSelector();
                FilterItems(searchInput.text);
            });
            
            GameObject effectObj5 = GameObject.Instantiate(MysteryDice.DebugSubButtonPrefab, scrollContent);
            TMP_Text buttonText5 = effectObj5.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText5.text = $"Player Choose Ad";

            Button button5 = effectObj5.GetComponent<Button>();
            button5.onClick.AddListener(() =>
            {
                clearMainViewport();
                ForcePlayerAd();
                FilterItems(searchInput.text);
            });
        }
        OnPlayerFunctionsAdded?.Invoke(subScrollContent);
    }

    public static void TeleportPlayer(bool bring = false)
    {

        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();

        allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();


        foreach (var player in allPlayers)
        {
            if (!player.isPlayerControlled) continue;
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{player.playerUsername}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                var player2 = StartOfRound.Instance.localPlayerController;
                if (player2.isPlayerDead && bring)
                {
                    Networker.Instance.TeleportOrBringPlayerToPosServerRPC(
                        StartOfRound.Instance.spectateCamera.transform.position, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                }
                else
                {
                    Networker.Instance.TeleportOrBringPlayerServerRPC(
                        Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController), Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player), bring);
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
    public static void ForcePlayerAd()
    {

        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();

        allPlayers = allPlayers
            .GroupBy(x => x.playerUsername)
            .Select(g => g.First())
            .OrderBy(x => x.playerUsername)
            .ToList();


        foreach (var player in allPlayers)
        {
            if (!player.isPlayerControlled) continue;
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{player.playerUsername}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Networker.Instance.ShowAdMenuServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player));
            });

        }
    }

    public static void UnlockSomething()
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        Terminal terminal = GameObject.FindFirstObjectByType<Terminal>();
        foreach (var item in StartOfRound.Instance.unlockablesList.unlockables)
        {
            if(item.hasBeenUnlockedByPlayer) continue;
            
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            buttonText.text = item.unlockableName;
            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                StartOfRound.Instance.BuyShipUnlockableServerRpc(StartOfRound.Instance.unlockablesList.unlockables.IndexOf(item), terminal.groupCredits);
                CloseSelectMenu();
            });
        }
    }

    public static void ForceSuitPlayerSelector()
    {

        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

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
            if (!player.isPlayerControlled) continue;
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{player.playerUsername}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                clearMainViewport();
                ForceSuitSuitSelector(player);
                FilterItems(searchInput.text);
            });
        }
    }
    public static void ForceSuitSuitSelector(PlayerControllerB players)
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

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
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            buttonText.text = $"{SuitName}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Networker.Instance.suitStuffServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,players), suit.syncedSuitID.Value);
                CloseSelectMenu();
            });
        }

        GameObject effectObj2 = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
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
            clearMainViewport();
            ForceSuitSuitSelector(players);
            FilterItems(searchInput.text);
        });
    }

    public static void RefreshRevives()
    {
        clearMainViewport();
        RevivePlayer();
        FilterItems(searchInput.text);
    }

    public static void RevivePlayer()
    {
        ReviveOpen = false;
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

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
            if (!player.isPlayerDead) continue;
            count++;
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"{player.playerUsername}";

            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                clearMainViewport();
                CloseSelectMenu();
                RevivePlayer();
                FilterItems(searchInput.text);
                Networker.Instance.RevivePlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player),
                    StartOfRound.Instance.middleOfShipNode.position);
                string txtToSay = "";
                txtToSay = $"Revived {player.playerUsername}";
                Misc.SafeTipMessage($"Revival", txtToSay);
            });

        }

        if (count == 0)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
            buttonText.text = $"No Dead Players";
        }
    }
    public static void ShowSelectMenu(bool fromSaint = false)
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        List<IEffect> effects = getOrdered();

        foreach (IEffect effect in effects)
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            FavoriteEffectManager.FavoriteData favoritesData = FavoriteEffectManager.LoadFavorites();
            List<string> favoriteEffectNames = favoritesData.Favorites;

            bool isFavorite = favoriteEffectNames.Contains(effect.Name);
            string favoriteMarker = isFavorite ? "*" : "";

            buttonText.text = $"{favoriteMarker} {effect.Name} [{effect.Outcome}] {favoriteMarker}";
            buttonText.color = isFavorite ? FavoriteTextColor : TextColor;
            //buttonText.outlineColor = Color.black;
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
                CloseSelectMenu(fromSaint);
                effect.Use();
                bool useCustom = false;
                string nameToUse = "";
                if (fromMe)
                {
                    useCustom = true;
                    nameToUse = StartOfRound.Instance.localPlayerController.playerUsername.ToString();
                }
                if (fromYippee)
                {
                    useCustom = true;
                    nameToUse = "An Enemy";
                }
                if (fromRandom)
                {
                    useCustom = true;
                    nameToUse = Misc.GetRandomPlayer().playerUsername.ToString();
                }
                if (useCustom)
                {
                    Networker.Instance.LogEffectsToOwnerServerRPC(
                        nameToUse,
                        effect.Name,
                        (int)(effect.Outcome) + 1
                    );
                }
            });
            RightClickHandler2 rightClickHandler = effectObj.AddComponent<RightClickHandler2>();
            rightClickHandler.effectName = effect.Name;
            rightClickHandler.category = "Favorites";
            rightClickHandler.full = full;
            rightClickHandler.complete = complete;
            rightClickHandler.fromSaint = fromSaint;
        }
    } 
    
    public static void ShowSoundMenu()
    {

        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        foreach (var entry in MysteryDice.sounds.OrderBy(x => x.Key))
        {
            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            buttonText.text = entry.Key;
            //buttonText.outlineColor = Color.black;
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
    public static void ShowJumpscareMenu()
    {
        Transform scrollContent = EffectMenu.transform.Find("DebugMenu/Background/Scroll View/Viewport/Content");

        for (int i = 0; i < MysteryDice.JumpscareScript.scarePairs.Count; i++)
        {
            var entry = MysteryDice.JumpscareScript.scarePairs[i];

            GameObject effectObj = GameObject.Instantiate(MysteryDice.DebugMenuButtonPrefab, scrollContent);
            TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();

            buttonText.text = entry.Key != null ? entry.Key.name : "(null)";
            buttonText.outlineWidth = 1;

            buttonText.fontSize = buttonText.text.Length > 20 ? 12 : 16;

            int index = i;
            Button button = effectObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                CloseSelectMenu();
                Networker.Instance.JumpscareAllServerRPC(index);
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
    public static void CloseSelectMenu(bool force = false) // 
    {
        if (MysteryDice.DebugMenuClosesAfter.Value || force)
        {
            if (EffectMenu != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Destroy(EffectMenu);
                if(MysteryDice.LockDebugUI.Value) StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = false;
                OnSpecialFunctionsAdded = null;
                OnSpawnFunctionsAdded = null;
                OnPlayerFunctionsAdded = null;
            }
        }
    }

    
    public class DebugMenuController : MonoBehaviour
    {
        public GameObject EffectMenu;
        private InputAction escAction;

        private void Awake()
        {
            escAction = new InputAction(binding: "<Keyboard>/escape");
            escAction.performed += ctx => CloseMenu();
            escAction.Enable();
        }

        private void OnDestroy()
        {
            escAction.Disable();
        }

        public void CloseMenu()
        {
            if (EffectMenu != null)
            {
                Destroy(EffectMenu);
                EffectMenu = null;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                if(MysteryDice.LockDebugUI.Value) StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = false;
            }
        }
    }
   public class RightClickHandler2 : MonoBehaviour, IPointerClickHandler
    {
    public string effectName;
    public string category;
    public bool full;
    public bool complete;
    public bool fromSaint = false;

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
                case "FavoriteItems":
                    ToggleFavorite(effectName, favorites.FavoriteItems);
                    break;
                case "FavoriteAdItems":
                    ToggleFavorite(effectName, favorites.FavoriteItems);
                    break;
            }

            FavoriteEffectManager.SaveFavorites(favorites);
            clearMainViewport();

         
            switch (category)
            {
                case "Favorites":
                    ShowSelectMenu(fromSaint);
                    break;
                case "FavoriteEnemies":
                    spawnEnemy();
                    break;
                case "FavoriteTraps":
                    spawnTrap();
                    break;
                case "FavoriteScraps":
                    spawnScrap();
                    break;
                case "FavoriteShopItems":
                    spawnShopItems();
                    break;
                case "FavoriteItems":
                    spawnAnyItem();
                    break;
                case "FavoriteAdItems":
                    spawnAnyItem();
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


}
