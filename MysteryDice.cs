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

namespace MysteryDice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MysteryDice : BaseUnityPlugin
    {
        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice Updated";
        private const string modVersion = "1.3.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle LoadedAssets;

        public static InputAction debugMenuAction = null;
        public static GameObject NetworkerPrefab, JumpscareCanvasPrefab, JumpscareOBJ, PathfinderPrefab, EffectMenuPrefab, EffectMenuButtonPrefab;
        public static Jumpscare JumpscareScript;

        public static AudioClip ExplosionSFX, DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, AlarmSFX, PurrSFX;
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static string debugKey = "<Keyboard>/numpadMinus";
        public static Item DieEmergency, DieGambler, DieChronos, DieSacrificer, DieSaint, DieRusty, PathfinderSpawner;
        public static ConfigFile BepInExConfig = null;
        public static bool lethalThingsPresent = false;
        public static Assembly lethalThingsAssembly;
        public static bool LethalMonPresent = false;
        public static Assembly LethalMonAssembly;
        public static bool LCOfficePresent = false;
        public static Assembly LCOfficeAssembly;

        void Awake()
        {
            CustomLogger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            lethalThingsAssembly = GetAssembly("evaisa.lethalthings");
            lethalThingsPresent = IsModPresent("evaisa.lethalthings", "LethalThings compatablilty enabled!");
            //db(); //Enable this to get all assembly names
            LethalMonAssembly = GetAssembly("LethalMon");
            LethalMonPresent = IsModPresent("LethalMon", "LethalMon compatablilty enabled!");
            LCOfficeAssembly = GetAssembly("Piggy.LCOffice");
            LCOfficePresent = IsModPresent("Piggy.LCOffice", "LCOffice compatablilty enabled!");
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Emergency Dice.cfg"), true);
            ModConfig();
            DieBehaviour.Config();

            NetcodeWeaver();
            
            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice"));
            
            ExplosionSFX = LoadedAssets.LoadAsset<AudioClip>("MineDetonate");
            MineSFX = LoadedAssets.LoadAsset<AudioClip>("MineTrigger");
            AwfulEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bell2");
            BadEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Bad1");
            GoodEffectSFX = LoadedAssets.LoadAsset<AudioClip>("Good2");
            JumpscareSFX = LoadedAssets.LoadAsset<AudioClip>("glitch");
            PurrSFX = LoadedAssets.LoadAsset<AudioClip>("purr");
            AlarmSFX = LoadedAssets.LoadAsset<AudioClip>("alarmcurse");

            WarningBracken = LoadedAssets.LoadAsset<Sprite>("bracken");
            WarningJester = LoadedAssets.LoadAsset<Sprite>("jester");
            WarningDeath = LoadedAssets.LoadAsset<Sprite>("death");
            WarningLuck = LoadedAssets.LoadAsset<Sprite>("luck");

            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.AddComponent<Networker>();

            EffectMenuPrefab = LoadedAssets.LoadAsset<GameObject>("Choose Effect");
            EffectMenuButtonPrefab = LoadedAssets.LoadAsset<GameObject>("Effect");

            JumpscareCanvasPrefab = LoadedAssets.LoadAsset<GameObject>("JumpscareCanvas");
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

            debugMenuAction = new InputAction(null, InputActionType.Value, debugKey);
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

        private void DebugMenu()
        {
            CustomLogger.LogInfo((object)"Button Hit");
            if (Networker.Instance.IsHost)
            {
                SelectEffect.ShowSelectMenu();
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

        public static void ModConfig()
        {
            ConfigEntry<bool> pussyMode = BepInExConfig.Bind<bool>(
                "Clientside",
                "Pussy mode",
                true,
                "Changes the jumpscare effect to a less scary one.");

            JumpscareGlitch.PussyMode = pussyMode.Value;

            ConfigEntry<float> minHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Min Force",
                30.0f,
                "Changes the minimum that hypershake can move you.");

            HyperShake.minForce = minHyperShake.Value;

            ConfigEntry<float> maxHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Max Force",
                100.0f,
                "Changes the maximum that hypershake can move you.");

            HyperShake.maxForce = maxHyperShake.Value;

            ConfigEntry<bool> randomSpinTime = BepInExConfig.Bind<bool>(
                "Misc",
                "Have a random spin time",
                true,
                "Makes the dice spin a random amount of time before rolling.");

            DieBehaviour.randomUseTimer = randomSpinTime.Value;

            ConfigEntry<bool> chronosUpdatedTimeOfDay = BepInExConfig.Bind<bool>(
                "Misc",
                "Updated Chronos Time",
                false,
                "Makes the Chronos die have better odds in the morning instead of equal odds in the morning.");

            ChronosDie.differentTimes = chronosUpdatedTimeOfDay.Value;

            ConfigEntry<bool> useDiceOutside = BepInExConfig.Bind<bool>(
                "Misc",
                "Use Dice Outside",
                false,
                "Allows the use of the Chronos and Gambler outside.");

            GamblerDie.useOutside = useDiceOutside.Value;
            ChronosDie.useOutside = useDiceOutside.Value;

            ConfigEntry<bool> debugDice = BepInExConfig.Bind<bool>(
                "Admin",
                "Show effects in the console",
                false,
                "Shows what effect has been rolled by the dice in the console. For debug purposes.");

            DieBehaviour.LogEffectsToConsole = debugDice.Value;


            ConfigEntry<bool> allowChatCommands = BepInExConfig.Bind<bool>(
                "Admin",
                "Allow chat commands",
                false,
                "Enables chat commands for the admin. Mainly for debugging.");

            ChatPatch.AllowChatDebug = allowChatCommands.Value;

            ConfigEntry<float> minNeckSpin = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin Min Speed",
                0.1f,
                "Changes the minimum speed that your neck can spin.");

            NeckSpin.minSpin = minNeckSpin.Value;

            ConfigEntry<float> maxNeckSpin = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin Max Speed",
                0.8f,
                "Changes the maximum speed that your neck can spin. ");

            NeckSpin.maxSpin = maxNeckSpin.Value; 
            
            ConfigEntry<int> neckRotations = BepInExConfig.Bind<int>(
                "NeckSpin",
                "NeckSpin Number of Rotations",
                -1,
                "Changes how many times your neck can rotate before it stops, -1 for infinite");
            
            ConfigEntry<float> rotationSpeedModifier = BepInExConfig.Bind<float>(
                "NeckSpin",
                "NeckSpin SpeedModifier",
                3f,
                "Changes the min and max speed if the Number of rotations isn't infinite");

            NeckSpin.numberOfRotations = neckRotations.Value;
            
            ConfigEntry<bool> useNeckBreakTimer = BepInExConfig.Bind<bool>(
                "NeckBreak",
                "Use Timer",
                true,
                "Use a timer for neck break instead of until the end of the round");

            NeckBreak.useTimer = useNeckBreakTimer.Value;

            ConfigEntry<int> minNeckBreakTimer = BepInExConfig.Bind<int>(
                "NeckBreak",
                "Min Break Time",
                30,
                "Sets the broken Neck Minimum Time");

            NeckBreak.setTimeMin = minNeckBreakTimer.Value;

            ConfigEntry<int> maxNeckBreakTimer = BepInExConfig.Bind<int>(
                "NeckBreak",
                "Max Break Time",
                60,
                "Sets the broken Neck Maximum Time");

            NeckBreak.setTimeMax = maxNeckBreakTimer.Value;

            ConfigEntry<string> adminKeybind = BepInExConfig.Bind<string>(
               "Admin",
               "Admin Keybind",
               "<Keyboard>/numpadMinus",
               "Button which opens the admin menu"); ;

            debugKey = adminKeybind.Value;

            ConfigEntry<string> DisplayResults = BepInExConfig.Bind<string>(
                "Misc",
                "Display Results",
                "Default",
                "Display the dice results or not \nAll - Shows all, None - shows none,\n Default, Shows the default ones, Random - Randomly shows them"); ;

            string DispRes = DisplayResults.Value;
            switch (DispRes.ToUpper())
            {
                case "ALL":
                    DieBehaviour.showE = DieBehaviour.ShowEffect.ALL;
                    break;
                case "NONE":
                    DieBehaviour.showE = DieBehaviour.ShowEffect.NONE;
                    break;
                case "RANDOM":
                    DieBehaviour.showE = DieBehaviour.ShowEffect.RANDOM;
                    break;
                default:
                    DieBehaviour.showE = DieBehaviour.ShowEffect.DEFAULT;
                    break;
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
            {Consts.Titan,Levels.LevelTypes.TitanLevel}
        };

        public static List<Item> RegisteredDice = new List<Item>();

        public static void LoadDice()
        {
            DieGambler = LoadedAssets.LoadAsset<Item>("MysteryDiceItem");

            DieGambler.minValue = 100;
            DieGambler.maxValue = 130;

            GamblerDie scriptMystery = DieGambler.spawnPrefab.AddComponent<GamblerDie>();
            scriptMystery.grabbable = true;
            scriptMystery.grabbableToEnemies = true;
            scriptMystery.itemProperties = DieGambler;

            RegisteredDice.Add(DieGambler);

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

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "This handy, unstable device might be your last chance to save yourself.\n\n" +
                "Rolls a number from 1 to 6:\n" +
                "-Rolling 6 teleports you and players standing closely near you to the ship with all your items.\n" +
                "-Rolling 4 or 5 teleports you to the ship with all your items.\n" +
                "-Rolling 3 might be bad, or might be good. You decide? \n" +
                "-Rolling 2 will causes some problems\n" +
                "-You dont want to roll a 1\n";

            Items.RegisterShopItem(DieEmergency, null, null, node, 200);

            Dictionary<(string,string),int> DefaultSpawnRates = new Dictionary<(string, string), int>();


            ///This probably could be refactored

            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Default), 25);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Experimentation), 13);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Assurance), 13);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Vow), 15);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Offense), 17);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.March), 17);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Rend), 33);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Dine), 46);
            DefaultSpawnRates.Add((DieGambler.itemName, Consts.Titan), 30);

            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Default), 23);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Experimentation), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Assurance), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Vow), 17);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Offense), 25);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.March), 25);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Rend), 22);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Dine), 41);
            DefaultSpawnRates.Add((DieChronos.itemName, Consts.Titan), 33);

            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Default), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Experimentation), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Assurance), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Vow), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Offense), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.March), 20);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Rend), 35);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Dine), 38);
            DefaultSpawnRates.Add((DieSacrificer.itemName, Consts.Titan), 23);

            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Default), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Experimentation), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Assurance), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Vow), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Offense), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.March), 10);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Rend), 12);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Dine), 15);
            DefaultSpawnRates.Add((DieSaint.itemName, Consts.Titan), 12);

            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Default), 18);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Experimentation), 15);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Assurance), 15);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Vow), 5);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Offense), 18);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.March), 5);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Rend), 16);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Dine), 26);
            DefaultSpawnRates.Add((DieRusty.itemName, Consts.Titan), 14);


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
                        "Sets how often this item spawns on this level. 0-10 is very rare, 10-25 is rare, 25+ is common. This is only from my observations."
                    );
                    Items.RegisterScrap(die, rate.Value, level.Value);
                }

                Items.RegisterScrap(die, defaultRate.Value, Levels.LevelTypes.All);
            }
        }
    }
}
