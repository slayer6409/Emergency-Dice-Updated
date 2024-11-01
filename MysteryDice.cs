using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.IO;
using LethalLib.Modules;
using MysteryDice.Effects;
using MysteryDice.Visual;
using MysteryDice.Dice;
using System;
using BepInEx.Configuration;
using MysteryDice.Patches;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using System.Diagnostics;
using UnityEngine.InputSystem;
using static MysteryDice.Dice.DieBehaviour;

namespace MysteryDice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Surfaced", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("LCTarotCard", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("evaisa.lethalthings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("x753.Mimics", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Chaos.Diversity", BepInDependency.DependencyFlags.SoftDependency)]
    public class MysteryDice : BaseUnityPlugin
    {
        public static bool DEBUGMODE = false;

        public enum chatDebug { HostOnly, Everyone, None};

        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice Updated";
        private const string modVersion = "1.5.18";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle LoadedAssets, LoadedAssets2;

        public static InputAction debugMenuAction = null;
        public static GameObject NetworkerPrefab, JumpscareCanvasPrefab, JumpscareOBJ, PathfinderPrefab, EffectMenuPrefab, EffectMenuButtonPrefab;
        public static Jumpscare JumpscareScript;

        public static AudioClip ExplosionSFX, DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, MeetingSFX, DawgSFX, AlarmSFX, PurrSFX, JawsSFX;
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static Item DieEmergency, DieGambler, DieChronos, DieSacrificer, DieSaint, DieRusty, PathfinderSpawner;
        public static ConfigFile BepInExConfig = null;
        public static bool lethalThingsPresent = false;
        public static Assembly lethalThingsAssembly;
        public static bool LethalMonPresent = false;
        public static Assembly LethalMonAssembly;
        public static bool LCOfficePresent = false;
        public static bool CodeRebirthPresent = false;
        public static bool SurfacedPresent = false;
        public static bool LCTarotCardPresent = false;
        public static bool TakeyPlushPresent = false;
        public static bool DiversityPresent = false;
        public static Assembly LCOfficeAssembly;
        public static bool terminalLockout = false;
        public static CustomConfigs customCfg;

        #region configEntry
        public static ConfigEntry<bool> pussyMode;
        public static ConfigEntry<float> minHyperShake;
        public static ConfigEntry<float> maxHyperShake;
        public static ConfigEntry<bool> randomSpinTime;
        public static ConfigEntry<bool> chronosUpdatedTimeOfDay;
        public static ConfigEntry<bool> useDiceOutside;
        public static ConfigEntry<bool> debugDice;
        public static ConfigEntry<chatDebug> debugChat;
        public static ConfigEntry<bool> allowChatCommands;
        public static ConfigEntry<float> eggExplodeTime;
        public static ConfigEntry<float> minNeckSpin;
        public static ConfigEntry<float> maxNeckSpin;
        public static ConfigEntry<int> neckRotations;
        public static ConfigEntry<float> rotationSpeedModifier;
        public static ConfigEntry<bool> useNeckBreakTimer;
        public static ConfigEntry<bool> debugMenuShowsAll;
        public static ConfigEntry<int> minNeckBreakTimer;
        public static ConfigEntry<int> maxNeckBreakTimer;
        public static ConfigEntry<int> hyperShakeTimer;
        public static ConfigEntry<int> EmergencyDiePrice;
        public static ConfigEntry<int> CustomEnemyEventCount;
        public static ConfigEntry<int> CustomItemEventCount;
        public static ConfigEntry<int> CustomTrapEventCount;
        public static ConfigEntry<float> BoombaEventSpeed;
        public static ConfigEntry<string> adminKeybind;
        public static ConfigEntry<bool> debugButton;
        public static ConfigEntry<bool> superDebugMode;
        public static ConfigEntry<bool> DebugLogging;
        public static ConfigEntry<bool> DicePosUpdate;
        public static ConfigEntry<bool> BetterDebugMenu;
        public static ConfigEntry<bool> DisableSizeBased;
        public static ConfigEntry<bool> DieEmergencyAsScrap;
        public static ConfigEntry<DieBehaviour.ShowEffect> DisplayResults;

        public static void ModConfig()
        {
            pussyMode = BepInExConfig.Bind<bool>(
                "Clientside",
                "Pussy mode",
                true,
                "Changes the jumpscare effect to a less scary one.");

            DieEmergencyAsScrap = BepInExConfig.Bind<bool>(
                "Emergency Die",
                "Scrap",
                false,
                "Enables the Emergency Die to be scrap");

            debugButton = BepInExConfig.Bind<bool>(
                "Admin",
                "Debug Button",
                false,
                "Enables the debug button(Must be host)");

            superDebugMode = BepInExConfig.Bind<bool>(
                "Admin",
                "Super Debug",
                false,
                "You probably don't want this, it makes clients be able to use the menu");

            debugMenuShowsAll = BepInExConfig.Bind<bool>(
                "Admin",
                "Debug Menu Shows All Events",
                false,
                "Makes the debug menu show all the events even if turned off");

            BetterDebugMenu = BepInExConfig.Bind<bool>(
                "Admin",
                "Better Debug Menu",
                false,
                "Enables the Better Debug Menu");

            DisableSizeBased = BepInExConfig.Bind<bool>(
                "Misc",
                "Disable Size Based Stuff",
                false,
                "Disables size based things");
            
            debugDice = BepInExConfig.Bind<bool>(
                "Admin",
                "Show effects in the console",
                false,
                "Shows what effect has been rolled by the dice in the console. For debug purposes.");

            debugChat = BepInExConfig.Bind<chatDebug>(
                "Admin",
                "Show effects in the chat",
                chatDebug.None,
                "Shows what effect has been rolled by the dice in the chat. For debug purposes.");

            adminKeybind = BepInExConfig.Bind<string>(
               "Admin",
               "Admin Keybind",
               "<Keyboard>/numpadMinus",
               "Button which opens the admin menu");

            minHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Min Force",
                15.0f,
                "Changes the minimum that hypershake can move you.");

            maxHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Max Force",
                60.0f,
                "Changes the maximum that hypershake can move you.");

            hyperShakeTimer = BepInExConfig.Bind<int>(
                "Hypershake",
                "HyperShake Length",
                -1,
                "Changes how long until hypershake is done randomly going until you get the event again in seconds\n-1 to diable and have it go until the end of the round");

            randomSpinTime = BepInExConfig.Bind<bool>(
                "Misc",
                "Have a random spin time",
                true,
                "Makes the dice spin a random amount of time before rolling.");

            chronosUpdatedTimeOfDay = BepInExConfig.Bind<bool>(
                "Misc",
                "Updated Chronos Time",
                false,
                "Makes the Chronos die have better odds in the morning instead of equal odds in the morning.");

            useDiceOutside = BepInExConfig.Bind<bool>(
                "Misc",
                "Use Dice Outside",
                false,
                "Allows the use of the Chronos and Gambler outside.");

            allowChatCommands = BepInExConfig.Bind<bool>(
                "Admin",
                "Allow chat commands",
                false,
                "Enables chat commands for the admin. Mainly for debugging.");

            eggExplodeTime = BepInExConfig.Bind<float>(
                "Misc",
                "Egg Fountain Time",
                0.25f,
                "Sets how quickly each egg explodes in the fountain, set to 0 for all explode instantly");

            minNeckSpin = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin Min Speed",
                0.1f,
                "Changes the minimum speed that your neck can spin.");

            maxNeckSpin = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin Max Speed",
                0.8f,
                "Changes the maximum speed that your neck can spin. ");

            neckRotations = BepInExConfig.Bind<int>(
                "NeckSpin",
                "NeckSpin Number of Rotations",
                -1,
                "Changes how many times your neck can rotate before it stops, -1 for infinite");

            rotationSpeedModifier = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin SpeedModifier",
                3f,
                "Changes the min and max speed if the Number of rotations isn't infinite");

            useNeckBreakTimer = BepInExConfig.Bind<bool>(
                "NeckBreak",
                "Use Timer",
                true,
                "Use a timer for neck break instead of until the end of the round");

            CustomEnemyEventCount = BepInExConfig.Bind<int>(
                "Custom",
                "Custom Enemy Events",
                0,
                "Sets the Number of Custom Enemy Events");

            CustomItemEventCount = BepInExConfig.Bind<int>(
                "Custom",
                "Custom Item Events",
                0,
                "Sets the Number of Custom Item Events");

            CustomTrapEventCount = BepInExConfig.Bind<int>(
                "Custom",
                "Custom Trap Events",
                0,
                "Sets the Number of Custom Trap Events");

            minNeckBreakTimer = BepInExConfig.Bind<int>(
                "NeckBreak",
                "Min Break Time",
                30,
                "Sets the broken Neck Minimum Time");

            EmergencyDiePrice = BepInExConfig.Bind<int>(
                "Emergency Die",
                "Emergency Dice Price",
                200,
                "Sets the Price of the Emergency Die");

            maxNeckBreakTimer = BepInExConfig.Bind<int>(
                "NeckBreak",
                "Max Break Time",
                60,
                "Sets the broken Neck Maximum Time");

            DisplayResults = BepInExConfig.Bind<DieBehaviour.ShowEffect>(
                "Misc",
                "Display Results",
                DieBehaviour.ShowEffect.DEFAULT,
                "Display the dice results or not \nAll - Shows all, None - shows none,\n Default, Shows the default ones, Random - Randomly shows them");

            DicePosUpdate = BepInExConfig.Bind<bool>(
                "Admin",
                "Dice Position Update",
                false,
                "You don't want this on unless you want to help me figure out why gambler is being stupid");

            DebugLogging = BepInExConfig.Bind<bool>(
                "Admin",
                "Debug Logging",
                false,
                "This is so I can see what the names of a lot of things are, probably not useful for most people");


            //if (lethalThingsPresent)
            //{
            //    BoombaEventSpeed = BepInExConfig.Bind<float>(
            //   "Misc",
            //   "Boomba Event Speed",
            //   8.0f,
            //   "Sets the speed of the Speedy Boombas");
            //}
        }
        public static List<ConfigEntryBase> GetListConfigs()
        {
            List<ConfigEntryBase> toSend = new List<ConfigEntryBase>();
            //Everything Commented out should be client side or doesn't matter
            //toSend.Add(pussyMode);
            //toSend.Add(debugDice);
            //toSend.Add(adminKeybind);
            //toSend.Add(debugButton);
            toSend.Add(debugChat);
            toSend.Add(superDebugMode);
            toSend.Add(DieEmergencyAsScrap);
            toSend.Add(EmergencyDiePrice);
            toSend.Add(hyperShakeTimer);
            toSend.Add(minHyperShake);
            toSend.Add(maxHyperShake);
            toSend.Add(randomSpinTime);
            toSend.Add(chronosUpdatedTimeOfDay);
            toSend.Add(useDiceOutside);
            toSend.Add(allowChatCommands);
            toSend.Add(minNeckSpin);
            toSend.Add(maxNeckSpin);
            toSend.Add(neckRotations);
            toSend.Add(rotationSpeedModifier);
            toSend.Add(useNeckBreakTimer);
            toSend.Add(minNeckBreakTimer);
            toSend.Add(maxNeckBreakTimer);
            toSend.Add(DisplayResults);
            toSend.Add(SizeDifference.sizeOption);
            return toSend;
        }

        #endregion
        void Awake()
        {
            CustomLogger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            lethalThingsAssembly = GetAssembly("evaisa.lethalthings");
            lethalThingsPresent = IsModPresent("evaisa.lethalthings", "LethalThings compatablilty enabled!");
            LethalMonAssembly = GetAssembly("LethalMon"); //This was before I learned about soft dependencies lol
            LethalMonPresent = IsModPresent("LethalMon", "LethalMon compatablilty enabled!");
            LCOfficeAssembly = GetAssembly("Piggy.LCOffice"); //This was before I learned about soft dependencies lol
            LCOfficePresent = IsModPresent("Piggy.LCOffice", "LCOffice compatablilty enabled!");
            SurfacedPresent = IsModPresent("Surfaced", "Surfaced compatablilty enabled!");
            LCTarotCardPresent = IsModPresent("LCTarotCard", "LCTarotCard compatablilty enabled!");
            TakeyPlushPresent = IsModPresent("com.github.zehsteam.TakeyPlush", "TakeyPlush compatablilty enabled!");
            CodeRebirthPresent = IsModPresent("CodeRebirth", "CodeRebirth compatablilty enabled!");
            DiversityPresent = IsModPresent("Chaos.Diversity", "Diversity: Remastered compatablilty enabled!");
            //MimicsPresent = IsModPresent("x753.Mimics", "Mimics compatablilty enabled!");
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Emergency Dice.cfg"),true);
            ModConfig();
            InvisibleEnemy.Config();
            SizeDifference.Config();
            BlameGlitch.Config();
            if (SurfacedPresent)
            {
                Flinger.Config();
            }
            customCfg = new CustomConfigs(BepInExConfig);
            customCfg.GenerateConfigs(CustomEnemyEventCount.Value, CustomItemEventCount.Value, CustomTrapEventCount.Value);
            DieBehaviour.Config();


            //All config edits come before this
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
                ConfigManager.setupLethalConfig();
            NetcodeWeaver();

            if (superDebugMode.Value) db(); //Enable this to get all assembly names

            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice"));
            LoadedAssets2 = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice2"));
            
            ExplosionSFX = LoadedAssets.LoadAsset<AudioClip>("MineDetonate");
            MineSFX = LoadedAssets.LoadAsset<AudioClip>("MineTrigger");
            AwfulEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bell2");
            BadEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bad1");
            GoodEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Good2");
            JumpscareSFX = LoadedAssets.LoadAsset<AudioClip>("glitch");
            PurrSFX = LoadedAssets.LoadAsset<AudioClip>("purr");
            AlarmSFX = LoadedAssets.LoadAsset<AudioClip>("alarmcurse");
            MeetingSFX = LoadedAssets2.LoadAsset<AudioClip>("Meeting_Sound");
            DawgSFX = LoadedAssets2.LoadAsset<AudioClip>("Dawg");
            JawsSFX = LoadedAssets2.LoadAsset<AudioClip>("Jaws");

            WarningBracken = LoadedAssets.LoadAsset<Sprite>("bracken");
            WarningJester = LoadedAssets.LoadAsset<Sprite>("jester");
            WarningDeath = LoadedAssets.LoadAsset<Sprite>("death");
            WarningLuck = LoadedAssets.LoadAsset<Sprite>("luck");

            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.AddComponent<Networker>();

            EffectMenuPrefab = LoadedAssets.LoadAsset<GameObject>("Choose Effect");
            EffectMenuButtonPrefab = LoadedAssets.LoadAsset<GameObject>("Effect");

            JumpscareCanvasPrefab = LoadedAssets2.LoadAsset<GameObject>("JumpscareCanvas");
            JumpscareCanvasPrefab.AddComponent<Jumpscare>();

            PathfinderPrefab = LoadedAssets.LoadAsset<GameObject>("Pathfinder");
            PathfinderPrefab.AddComponent<Pathfinder.PathfindBehaviour>();

            PathfinderSpawner = LoadedAssets.LoadAsset<Item>("Pathblob");

            Pathfinder.BlobspawnerBehaviour scriptBlobspawner = PathfinderSpawner.spawnPrefab.AddComponent<Pathfinder.BlobspawnerBehaviour>();
            scriptBlobspawner.grabbable = true;
            scriptBlobspawner.grabbableToEnemies = true;
            scriptBlobspawner.itemProperties = PathfinderSpawner;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderSpawner.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderPrefab);

            LoadDice();

            debugMenuAction = new InputAction(null, InputActionType.Value, adminKeybind.Value);
            debugMenuAction.performed += delegate
            {
                DebugMenu();
            };
            debugMenuAction.Enable();
            harmony.PatchAll();
            CustomLogger.LogInfo("The Emergency Dice mod was initialized!");
        }

        private void db()
        {
            foreach (var e in Chainloader.PluginInfos)
            {
                CustomLogger.LogInfo($"{e}");
            }
        }

        public static void DebugMenu(bool bypassButton = false)
        {
            if (superDebugMode.Value)
            {
                SelectEffect.showDebugMenu(true, true);
                return;
            }
            if (Networker.Instance != null && ((Networker.Instance.IsHost && (debugButton.Value||bypassButton)) || GameNetworkManager.Instance.localPlayerController.playerSteamId == 76561198077184650))
            {
                if (BetterDebugMenu.Value)
                {
                    if (GameNetworkManager.Instance.localPlayerController.playerSteamId == 76561198077184650) SelectEffect.showDebugMenu(true, true);
                    else if (debugMenuShowsAll.Value) SelectEffect.showDebugMenu(true, false);
                    else SelectEffect.showDebugMenu(false, false);
                }
                else
                {
                    if (GameNetworkManager.Instance.localPlayerController.playerSteamId == 76561198077184650) SelectEffect.ShowSelectMenu(true, true);
                    else if (debugMenuShowsAll.Value) SelectEffect.ShowSelectMenu(true, false);
                    else SelectEffect.ShowSelectMenu(false, false);
                }
                
            }   
        }

        public static Assembly GetAssembly(string name)
        {
            if (Chainloader.PluginInfos.ContainsKey(name))
            {
                return Chainloader.PluginInfos[name].Instance.GetType().Assembly;
            }
            return null;
        }
        private static bool IsModPresent(string name, string logMessage)
        {
            bool isPresent = Chainloader.PluginInfos.ContainsKey(name);
            if(isPresent)MysteryDice.CustomLogger.LogMessage(logMessage);
            return isPresent;
        }
        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

       
    

        public static Dictionary<string, Levels.LevelTypes> RegLevels = new Dictionary<string, Levels.LevelTypes>
        {
            {Consts.Experimentation,Levels.LevelTypes.ExperimentationLevel},
            {Consts.Assurance,Levels.LevelTypes.AssuranceLevel},
            {Consts.Vow,Levels.LevelTypes.VowLevel},
            {Consts.Offense,Levels.LevelTypes.OffenseLevel},
            {Consts.March,Levels.LevelTypes.MarchLevel},
            {Consts.Rend,Levels.LevelTypes.RendLevel},
            {Consts.Dine,Levels.LevelTypes.DineLevel},
            {Consts.Titan,Levels.LevelTypes.TitanLevel},
            {Consts.Adamance,Levels.LevelTypes.AdamanceLevel},
            {Consts.Artifice,Levels.LevelTypes.ArtificeLevel},
            {Consts.Embrion,Levels.LevelTypes.EmbrionLevel}
        };

        public static List<Item> RegisteredDice = new List<Item>();

        public static void LoadDice()
        {

            DieEmergency = LoadedAssets.LoadAsset<Item>("Emergency Dice Script");
            DieEmergency.highestSalePercentage = 80;

            EmergencyDie scriptEmergency = DieEmergency.spawnPrefab.AddComponent<EmergencyDie>();
            scriptEmergency.grabbable = true;
            scriptEmergency.grabbableToEnemies = true;
            scriptEmergency.itemProperties = DieEmergency;

            RegisteredDice.Add(DieEmergency);

            ///

            DieChronos = LoadedAssets.LoadAsset<Item>("Chronos");
            DieChronos.minValue = 120;
            DieChronos.maxValue = 140;

            ChronosDie scriptChronos = DieChronos.spawnPrefab.AddComponent<ChronosDie>();
            scriptChronos.grabbable = true;
            scriptChronos.grabbableToEnemies = true;
            scriptChronos.itemProperties = DieChronos;

            RegisteredDice.Add(DieChronos);

            ///

            DieGambler = LoadedAssets.LoadAsset<Item>("MysteryDiceItem");

            DieGambler.minValue = 100;
            DieGambler.maxValue = 130;

            GamblerDie scriptMystery = DieGambler.spawnPrefab.AddComponent<GamblerDie>();
            scriptMystery.grabbable = true;
            scriptMystery.grabbableToEnemies = true;
            scriptMystery.itemProperties = DieGambler;

            RegisteredDice.Add(DieGambler);

            DieSacrificer = LoadedAssets.LoadAsset<Item>("Sacrificer");
            DieSacrificer.minValue = 170;
            DieSacrificer.maxValue = 230;

            SacrificerDie scriptSacrificer = DieSacrificer.spawnPrefab.AddComponent<SacrificerDie>();
            scriptSacrificer.grabbable = true;
            scriptSacrificer.grabbableToEnemies = true;
            scriptSacrificer.itemProperties = DieSacrificer;

            RegisteredDice.Add(DieSacrificer);

            ///

            DieSaint = LoadedAssets.LoadAsset<Item>("Saint");
            DieSaint.minValue = 210;
            DieSaint.maxValue = 280;

            SaintDie scriptSaint = DieSaint.spawnPrefab.AddComponent<SaintDie>();
            scriptSaint.grabbable = true;
            scriptSaint.grabbableToEnemies = true;
            scriptSaint.itemProperties = DieSaint;

            RegisteredDice.Add(DieSaint);

            ///

            DieRusty = LoadedAssets.LoadAsset<Item>("Rusty");
            DieRusty.minValue = 90;
            DieRusty.maxValue = 160;

            RustyDie scriptRusty = DieRusty.spawnPrefab.AddComponent<RustyDie>();
            scriptRusty.grabbable = true;
            scriptRusty.grabbableToEnemies = true;
            scriptRusty.itemProperties = DieRusty;

            RegisteredDice.Add(DieRusty);

            ///


            if (EmergencyDiePrice.Value >= 0)
            {
                TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
                node.clearPreviousText = true;
                node.displayText = "This handy, unstable device might be your last chance to save yourself.\n\n" +
                    "Rolls a number from 1 to 6:\n" +
                    "-Rolling 6 teleports you and players standing closely near you to the ship with all your items.\n" +
                    "-Rolling 4 or 5 teleports you to the ship with all your items.\n" +
                    "-Rolling 3 might be bad, or might be good. You decide? \n" +
                    "-Rolling 2 will causes some problems\n" +
                    "-You dont want to roll a 1\n";
                 Items.RegisterShopItem(DieEmergency, null, null, node, EmergencyDiePrice.Value);
            }

            Dictionary<(string,string),int> DefaultSpawnRates = new Dictionary<(string, string), int>
            {
                { (DieGambler.itemName, Consts.Default), 25 },
                { (DieGambler.itemName, Consts.Experimentation), 13 },
                { (DieGambler.itemName, Consts.Assurance), 13 },
                { (DieGambler.itemName, Consts.Vow), 15 },
                { (DieGambler.itemName, Consts.Offense), 17 },
                { (DieGambler.itemName, Consts.March), 17 },
                { (DieGambler.itemName, Consts.Rend), 33 },
                { (DieGambler.itemName, Consts.Dine), 46 },
                { (DieGambler.itemName, Consts.Titan), 30 },
                { (DieGambler.itemName, Consts.Adamance), 21 },
                { (DieGambler.itemName, Consts.Artifice), 43 },
                { (DieGambler.itemName, Consts.Embrion), 60 },
                { (DieChronos.itemName, Consts.Default), 23 },
                { (DieChronos.itemName, Consts.Experimentation), 17 },
                { (DieChronos.itemName, Consts.Assurance), 17 },
                { (DieChronos.itemName, Consts.Vow), 17 },
                { (DieChronos.itemName, Consts.Offense), 25 },
                { (DieChronos.itemName, Consts.March), 25 },
                { (DieChronos.itemName, Consts.Rend), 22 },
                { (DieChronos.itemName, Consts.Dine), 41 },
                { (DieChronos.itemName, Consts.Titan), 33 },
                { (DieChronos.itemName, Consts.Adamance), 19 },
                { (DieChronos.itemName, Consts.Artifice), 40 },
                { (DieChronos.itemName, Consts.Embrion), 58 },
                { (DieSacrificer.itemName, Consts.Default), 20 },
                { (DieSacrificer.itemName, Consts.Experimentation), 20 },
                { (DieSacrificer.itemName, Consts.Assurance), 20 },
                { (DieSacrificer.itemName, Consts.Vow), 20 },
                { (DieSacrificer.itemName, Consts.Offense), 20 },
                { (DieSacrificer.itemName, Consts.March), 20 },
                { (DieSacrificer.itemName, Consts.Rend), 35 },
                { (DieSacrificer.itemName, Consts.Dine), 38 },
                { (DieSacrificer.itemName, Consts.Titan), 23 },
                { (DieSacrificer.itemName, Consts.Adamance), 20 },
                { (DieSacrificer.itemName, Consts.Artifice), 35 },
                { (DieSacrificer.itemName, Consts.Embrion), 41 },
                { (DieSaint.itemName, Consts.Default), 10 },
                { (DieSaint.itemName, Consts.Experimentation), 10 },
                { (DieSaint.itemName, Consts.Assurance), 10 },
                { (DieSaint.itemName, Consts.Vow), 10 },
                { (DieSaint.itemName, Consts.Offense), 10 },
                { (DieSaint.itemName, Consts.March), 10 },
                { (DieSaint.itemName, Consts.Rend), 12 },
                { (DieSaint.itemName, Consts.Dine), 15 },
                { (DieSaint.itemName, Consts.Titan), 12 },
                { (DieSaint.itemName, Consts.Adamance), 10 },
                { (DieSaint.itemName, Consts.Artifice), 15 },
                { (DieSaint.itemName, Consts.Embrion), 21 },
                { (DieRusty.itemName, Consts.Default), 18 },
                { (DieRusty.itemName, Consts.Experimentation), 15 },
                { (DieRusty.itemName, Consts.Assurance), 15 },
                { (DieRusty.itemName, Consts.Vow), 5 },
                { (DieRusty.itemName, Consts.Offense), 18 },
                { (DieRusty.itemName, Consts.March), 5 },
                { (DieRusty.itemName, Consts.Rend), 16 },
                { (DieRusty.itemName, Consts.Dine), 26 },
                { (DieRusty.itemName, Consts.Titan), 14 },
                { (DieRusty.itemName, Consts.Adamance), 16 },
                { (DieRusty.itemName, Consts.Artifice), 21 },
                { (DieRusty.itemName, Consts.Embrion), 38 }
            };

            if (DieEmergencyAsScrap.Value)
            {
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Default), 18);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Experimentation), 15);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Assurance), 15);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Vow), 5);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Offense), 18);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.March), 5);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Rend), 16);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Dine), 26);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Titan), 14);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Adamance), 16);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Artifice), 21);
                DefaultSpawnRates.Add((DieEmergency.itemName, Consts.Embrion), 38);
            }
           


            foreach (Item die in RegisteredDice)
            {
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(die.spawnPrefab);
                Utilities.FixMixerGroups(die.spawnPrefab);
            }

            foreach (Item die in RegisteredDice)
            {
                if (die == DieEmergency) continue;

                ConfigEntry<int> defaultRate = BepInExConfig.Bind<int>(
                    die.itemName + " Spawn rates",
                    "Default",
                    DefaultSpawnRates[(die.itemName, Consts.Default)],
                    "Default spawn rate for all levels. Mainly for setting up spawn rates for either new beta moons or modded ones."
                );
                

                foreach (KeyValuePair<string,Levels.LevelTypes> level in RegLevels)
                {
                    ConfigEntry<int> rate = BepInExConfig.Bind<int>(
                        die.itemName + " Spawn rates",
                        level.Key,
                        DefaultSpawnRates[(die.itemName, level.Key)],
                        "Sets how often this item spawns on this level. 0-10 is very rare, 10-25 is rare, 25+ is common. This is only from my observations. -Theronguard (These numbers are with no modded scrap from my observations - Slayer)"
                    );
                    Items.RegisterScrap(die, rate.Value, level.Value);
                }
                Items.RegisterScrap(die, defaultRate.Value, Levels.LevelTypes.All);
            }
        }
    }
    
}
