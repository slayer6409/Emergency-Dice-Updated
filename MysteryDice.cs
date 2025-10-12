using System;
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
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using LethalLib.Extras;
using LethalThings;
using MysteryDice.Gal;
using MysteryDice.MiscStuff;
using UnityEngine.AI;
//using MysteryDice.Gal;
using UnityEngine.InputSystem;
using Utilities = LethalLib.Modules.Utilities;

namespace MysteryDice
{
    //CompanyCruiser(Clone)
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Surfaced", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("LCTarotCard", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("evaisa.lethalthings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("x753.Mimics", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Chaos.Diversity", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Jordo.BombCollar", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dev.kittenji.NavMeshInCompany", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.swipez.melonloader.morecompany", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.fumiko.CullFactory", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("FlipMods.TooManyEmotes", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.xuuxiaolan.coderebirthlib", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("CodeRebirth", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Zaggy1024.PathfindingLib")]
    public class MysteryDice : BaseUnityPlugin
    {
        //public static bool DEBUGMODE = false;
        public static HashSet<ulong> admins = new HashSet<ulong>{   
            76561198077184650 /*Me*/,
            76561199094139351 /*Lizzie*/,
            76561198984467725 /*Glitch*/,
            76561198399127090 /*Xu*/,
            76561198086086035 /*Nut*/,
            76561198298343090 /*Mel*/
        };
        public static HashSet<ulong> revokedAdmins = new();
        
        public static readonly ulong slayerSteamID = 76561198077184650;
        
        internal static bool isAdmin=false;
        internal static bool triedRequestingAdmin=false;
        public enum chatDebug { Host, Everyone, None};
        private const string modGUID = "Theronguard.EmergencyDice";
        private const string modName = "Emergency Dice Updated";
        private const string modVersion = "1.13.2";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static AssetBundle  LoadedAssets, LoadedAssets2;

        
        internal static IngameKeybinds Keybinds = null!;
        
        public static UnlockableItemDef diceGalUnlockable;  
        public static GameObject NetworkerPrefab,
            JumpscareCanvasPrefab,
            SpiderCanvasPrefab,
            SpiderMoverPrefab,
            JumpscareOBJ,
            PathfinderPrefab,
            DebugMenuPrefab,
            NewSelectMenuPrefab,
            DebugMenuButtonPrefab,
            DebugSubButtonPrefab,
            DiceGal, 
            AgentObjectPrefab,
            PlayerNodeController;

        public static Material jobApplication;
        public static Material angyGlitch;
        public static Jumpscare JumpscareScript;

        //public static AudioClip ExplosionSFX, DetonateSFX, MineSFX, AwfulEffectSFX, BadEffectSFX, GoodEffectSFX, JumpscareSFX, MeetingSFX, DawgSFX, AlarmSFX, PurrSFX, JawsSFX, FireAlarmSFX, PaparazziSFX;
        public static Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
        public static Sprite WarningBracken, WarningJester, WarningDeath, WarningLuck;

        public static Item DieEmergency, DieGambler, DieChronos, DieSacrificer, DieSaint, DieRusty, DieSurfaced, DieCodeRebirth, DieSteve, PathfinderSpawner;
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
        public static bool TooManyEmotesPresent = false;
        public static bool DiversityPresent = false;
        public static bool NightOfTheLivingMimicPresent = false;
        public static bool NavMeshInCompanyPresent = false;
        public static bool BombCollarPresent = false;
        public static bool MoreCompanyPresent = false;
        public static bool LethalConfigPresent = false;
        public static Assembly LCOfficeAssembly;
        public static bool terminalLockout = false;
        public static CustomConfigs customCfg;
        //public static bool aprilFoolsMode = true;
        
        #region configEntry
        public static ConfigEntry<bool> aprilFoolsConfig;
        public static ConfigEntry<bool> pussyMode;
        public static ConfigEntry<float> minHyperShake;
        public static ConfigEntry<float> maxHyperShake;
        public static ConfigEntry<bool> randomSpinTime;
        public static ConfigEntry<bool> ConfigOnlyOwnerDisablesGal;
        public static ConfigEntry<bool> ConfigGalAutomatic;
        public static ConfigEntry<bool> chronosUpdatedTimeOfDay;
        public static ConfigEntry<bool> useDiceOutside;
        public static ConfigEntry<bool> debugDice;
        public static ConfigEntry<bool> allowChatCommands;
        public static ConfigEntry<float> eggExplodeTime;
        public static ConfigEntry<float> minNeckSpin;
        public static ConfigEntry<float> maxNeckSpin;
        public static ConfigEntry<int> neckRotations;
        public static ConfigEntry<float> rotationSpeedModifier;
        public static ConfigEntry<bool> useNeckBreakTimer;
        public static ConfigEntry<bool> debugMenuShowsAll;
        public static ConfigEntry<bool> yippeeUse;
        public static ConfigEntry<bool> insideJoke;
        public static ConfigEntry<int> minNeckBreakTimer;
        public static ConfigEntry<int> maxNeckBreakTimer;
        public static ConfigEntry<int> hyperShakeTimer;
        public static ConfigEntry<int> EmergencyDiePrice;
        public static ConfigEntry<int> CustomEnemyEventCount;
        public static ConfigEntry<int> CustomItemEventCount;
        public static ConfigEntry<int> CustomTrapEventCount;
        public static ConfigEntry<string> DebugMenuTextColor;
        public static ConfigEntry<int> DebugMenuTextAlpha;
        public static ConfigEntry<string> DebugMenuFavoriteTextColor;
        public static ConfigEntry<int> DebugMenuFavoriteTextAlpha;
        public static ConfigEntry<string> DebugMenuBackgroundColor;
        public static ConfigEntry<int> DebugMenuBackgroundAlpha;
        public static ConfigEntry<string> DebugMenuAccentColor;
        public static ConfigEntry<int> DebugMenuAccentAlpha;
        public static ConfigEntry<string> DebugButtonColor;
        public static ConfigEntry<string> cursedIDs;
        public static ConfigEntry<bool> cursedRandomly;
        public static ConfigEntry<bool> toggleCursed;
        public static ConfigEntry<int> DebugButtonAlpha;
        public static ConfigEntry<bool> BrutalMode;
        public static ConfigEntry<bool> BrutalChat;
        public static ConfigEntry<bool> SuperBrutalMode;
        public static ConfigEntry<int> brutalStartingScale;
        public static ConfigEntry<int> brutalMaxScale;
        public static ConfigEntry<int> ImFeelingLuckyCooldown;
        public static ConfigEntry<int> DevilDealCooldown;
        public static ConfigEntry<int> OnTheHouseCooldown;
        public static ConfigEntry<int> brutalScaleType;
        public static ConfigEntry<bool> showOwnScanNode;
        public static ConfigEntry<bool> Bald;
        public static ConfigEntry<bool> CopyrightFree;
        public static ConfigEntry<float> SoundVolume;
        //public static ConfigEntry<string> adminKeybind;
        public static ConfigEntry<bool> debugButton;
        public static ConfigEntry<bool> debugSpawnOnPlayer;
        public static ConfigEntry<bool> superDebugMode;
        public static ConfigEntry<bool> DebugLogging;
        public static ConfigEntry<bool> BetterDebugMenu;
        public static ConfigEntry<bool> LockDebugUI;
        //public static ConfigEntry<bool> DisableSizeBased;
        public static ConfigEntry<bool> doDiceExplosion;
        public static ConfigEntry<bool> DieEmergencyAsScrap;
        public static ConfigEntry<bool> DisableGal;
        public static ConfigEntry<int> GalPrice;
        public static ConfigEntry<bool> LoversOnStart;
        public static ConfigEntry<bool> DebugMenuClosesAfter;
        public static ConfigEntry<bool> TwitchEnabled;
        public static ConfigEntry<string> DisplayResults;
        public static ConfigEntry<string> debugChat;
        public static ConfigEntry<bool> deadAds;

        public static void ModConfig()
        {
            pussyMode = BepInExConfig.Bind<bool>(
                "Clientside",
                "Pussy mode",
                true,
                "Changes the jumpscare effect to a less scary one.");
            deadAds = BepInExConfig.Bind<bool>(
                "Clientside",
                "Dead Ads",
                true,
                "Do Ads play while dead.");

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
           
            LockDebugUI = BepInExConfig.Bind<bool>(
                "New Debug",
                "Lock input",
                true,
                "Locks input while the UI is open");

            // DisableSizeBased = BepInExConfig.Bind<bool>(
            //     "Misc",
            //     "Disable Size Based Stuff",
            //     false,
            //     "Disables size based things");
            
            debugDice = BepInExConfig.Bind<bool>(
                "Admin",
                "Show effects in the console",
                false,
                "Shows what effect has been rolled by the dice in the console. For debug purposes.");

            debugChat = BepInExConfig.Bind<string>(
                "Admin",
                "Show effects in the chat",
                chatDebug.None.ToString(),
                "Shows what effect has been rolled by the dice in the chat. For debug purposes.\n" +
                "Options are: Host, Everyone, None");
            
            DebugMenuTextColor = BepInExConfig.Bind<string>(
                "New Debug",
                "Text Color",
                "#F581FA",
                "Sets the text color of the Debug Menu.");
             DebugMenuTextAlpha = BepInExConfig.Bind<int>(
                "New Debug",
                "Text Alpha",
                100,
                "Sets the text alpha of the Debug Menu.");
            
             debugSpawnOnPlayer = BepInExConfig.Bind<bool>(
                "New Debug",
                "debugSpawnOnPlayer",
                true,
                "Spawn Enemy On Player with the Debug Menu."); 
             
             ConfigOnlyOwnerDisablesGal = BepInExConfig.Bind<bool>(
                "Gal",
                "Only Owner Disables Gal",
                true,
                "Makes it to where only the owner can disable the Gal");
             ConfigGalAutomatic = BepInExConfig.Bind<bool>(
                "Gal",
                "Auto Activate",
                false,
                "Makes the Gal automatically activate");
             DisableGal = BepInExConfig.Bind<bool>(
                "Gal",
                "Disable Completely",
                false,
                "Makes the gal not show up in the shop, and adds the gal effects to the dice pool");
             GalPrice = BepInExConfig.Bind<int>(
                "Gal",
                "Price",
                1777,
                "Sets the price of the Gal.");
             
             ImFeelingLuckyCooldown = BepInExConfig.Bind<int>(
                "Gal",
                "Im Feeling Lucky Cooldown",
                120,
                "How long is the cooldown for I'm Feeling Lucky Gal Effect");
             DevilDealCooldown = BepInExConfig.Bind<int>(
                "Gal",
                "Devil Deal Cooldown",
                90,
                "How long is the cooldown for Devil Deal Gal Effect");
             OnTheHouseCooldown = BepInExConfig.Bind<int>(
                "Gal",
                "On The House Cooldown",
                300,
                "How long is the cooldown for On The House Gal Effect");
             
            DebugMenuFavoriteTextColor = BepInExConfig.Bind<string>(
                "New Debug",
                "Favorite Text Color",
                "#F53548",
                "Sets the favorite text color of the Debug Menu.");
            
            DebugMenuFavoriteTextAlpha = BepInExConfig.Bind<int>(
                "New Debug",
                "Favorite Text Alpha",
                100,
                "Sets the favorite alpha of the Debug Menu.");

            DebugMenuBackgroundColor = BepInExConfig.Bind<string>(
                "New Debug",
                "Background Color",
                "#270051",
                "Sets the background color of the Debug Menu.");
            DebugMenuBackgroundAlpha = BepInExConfig.Bind<int>(
                "New Debug",
                "Background Alpha",
                46,
                "Sets the background alpha of the Debug Menu.");

            DebugMenuAccentColor = BepInExConfig.Bind<string>(
                "New Debug",
                "Accent Color",
                "#A34EFF",
                "Sets the accent color of the Debug Menu.");
            DebugMenuAccentAlpha = BepInExConfig.Bind<int>(
                "New Debug",
                "Accent Alpha",
                25,
                "Sets the accent alpha of the Debug Menu.");

            DebugButtonColor = BepInExConfig.Bind<string>(
                "New Debug",
                "Button Color",
                "#A447FF",
                "Sets the button color of the Debug Menu.");
            
            DebugButtonAlpha = BepInExConfig.Bind<int>(
                "New Debug",
                "Button Alpha",
                80,
                "Sets the button alpha of the Debug Menu.");


            // adminKeybind = BepInExConfig.Bind<string>(
            //    "Admin",
            //    "Admin Keybind",
            //    "<Keyboard>/numpadMinus",
            //    "Button which opens the admin menu");

            minHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Min Force",
                15.0f,
                "Changes the minimum that hypershake can move you.");
            Bald = BepInExConfig.Bind<bool>(
                "New Debug",
                "Bald",
                false,
                "Bald");

            maxHyperShake = BepInExConfig.Bind<float>(
                "Hypershake",
                "HyperShake Max Force",
                60.0f,
                "Changes the maximum that hypershake can move you.");
            
            LoversOnStart = BepInExConfig.Bind<bool>(
                "Misc",
                "Lovers On Start",
                false,
                "Assigns New Lovers on each round");
            
            insideJoke = BepInExConfig.Bind<bool>(
                "Misc",
                "Inside Joke",
                false,
                "Turns on inside jokes for stuff");
            
            aprilFoolsConfig = BepInExConfig.Bind<bool>(
                "Misc",
                "April Fools",
                false,
                "Toggles april fools mode");
            
            CopyrightFree = BepInExConfig.Bind<bool>(
                "Clientside",
                "Copyright Free",
                false,
                "Removes Copyright sounds over 10 seconds");
            CopyrightFree.SettingChanged += freebirdChange;
            
            SoundVolume = BepInExConfig.Bind<float>(
                "Clientside",
                "Sound Volume",
                0.75f,
                "Sets the volume for most sounds/music from this mod");
            SoundVolume.SettingChanged += setAudio;
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
                true,
                "Makes the Chronos die have better odds in the morning instead of equal odds in the morning.");
            
            doDiceExplosion = BepInExConfig.Bind<bool>(
                "Misc",
                "Do Dice Explosion",
                true,
                "If the dice explode after rolling or not");
            
            TwitchEnabled = BepInExConfig.Bind(
                "Twitch",
                "Enable Twitch Integration",
                false,
                "If Dice Twitch Integration is enabled (Needs TwitchChatAPI)");

            
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
            yippeeUse = BepInExConfig.Bind<bool>(
                "Misc",
                "Yippee Use Dice",
                true,
                "Makes it to where Hoarding bugs can use dice or not");

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
            
            showOwnScanNode = BepInExConfig.Bind<bool>(
                "Clientside",
                "Show own Scan Node",
                true,
                "Makes it to where you can see your own scan node or not (only affects certain people for now)");

            showOwnScanNode.SettingChanged += (s, e) =>
            {
                Misc.ToggleAllScanPlayerNodes();
            };
            
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

            DisplayResults = BepInExConfig.Bind<string>(
                "Misc",
                "Display Results",
                DieBehaviour.ShowEffect.ALL.ToString(),
                "Display the dice results or not:\nALL - Shows all, NONE - shows none,\nDEFAULT - shows the default ones, RANDOM - randomly shows them");

            DebugLogging = BepInExConfig.Bind<bool>(
                "Admin",
                "Debug Logging",
                false,
                "This is so I can see what the names of a lot of things are, probably not useful for most people");
           
            DebugMenuClosesAfter = BepInExConfig.Bind<bool>(
                "Misc",
                "Debug Menu Closes After",
                true,
                "Makes it to where the debug menu closes after selecting an effect, False to stay open");
            toggleCursed = BepInExConfig.Bind<bool>(
                "Misc",
                "ToggleCursed",
                false,
                "If set to true makes the cursed IDs and cursed randomly actually do stuff :D");
            cursedIDs = BepInExConfig.Bind<string>(
                "Misc",
                "Cursed IDs",
                "76561198984467725",
                "Curses players whose steam IDs you put in here seperated by a comma");
            cursedRandomly = BepInExConfig.Bind<bool>(
                "Misc",
                "Cursed Randomly",
                false,
                "Makes it to where random players are picked every quota for curses instead of the Cursed IDs List");
            BrutalMode = BepInExConfig.Bind<bool>(
                "Brutal",
                "Brutal Mode",
                false,
                "Makes it to where the Random Dice Events Happen at the start of the round\nThis was Requested by a friend");
            BrutalChat = BepInExConfig.Bind<bool>(
                "Brutal",
                "Brutal Chat",
                true,
                "Makes it to where the Brutal Events show in chat");
            SuperBrutalMode = BepInExConfig.Bind<bool>(
                "Brutal",
                "Super Brutal Mode",
                false,
                "Makes it to where the Random Dice Events Happen randomly");
            brutalStartingScale = BepInExConfig.Bind<int>(
                "Brutal",
                "Brutal Starting Scale",
                1,
                "How many events it starts with");
            brutalMaxScale = BepInExConfig.Bind<int>(
                "Brutal",
                "Brutal Max Scale",
                7,
                "Sets the Max number of events it starts with");
            brutalScaleType = BepInExConfig.Bind<int>(
                "Brutal",
                "Brutal Scale Type",
                0,
                "Sets how it scales" +
                "\n0: Scales based off how many days, maxing out at 50 days" +
                "\n1: Scales based off how much scrap is on the ship" +
                "\n2: Scales based off how many days survived in a row" +
                "\n3: Scales based off a combo of 0 and 1" +
                "\n4: Disable Scaling");
            

            //if (lethalThingsPresent)
            //{
            //    BoombaEventSpeed = BepInExConfig.Bind<float>(
            //   "Misc",
            //   "Boomba Event Speed",
            //   8.0f,
            //   "Sets the speed of the Speedy Boombas");
            //}
        }

        public static void setAudio(object sender, System.EventArgs e)
        {
            if(Networker.Instance == null) return;
            foreach (var audioSource in Networker.Instance.AudioSources)
            {
                audioSource.volume = SoundVolume.Value;
            }
            foreach (var audioSource in Networker.Instance.FreebirdAudioSources)
            {
                audioSource.volume = SoundVolume.Value;
            }
        }
        public static void freebirdChange(object sender, System.EventArgs e)
        {
            if(Networker.Instance == null) return;
            
            foreach (var audioSource in Networker.Instance.FreebirdAudioSources)
            {
                if (MysteryDice.CopyrightFree.Value)
                {
                    audioSource.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("SpazzmaticaPolka");
                }
                else
                {
                    audioSource.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("Freebird");
                }
                audioSource.Play();
            }
        }
        public static List<ConfigEntryBase> GetListConfigs()
        {
            List<ConfigEntryBase> toSend = new List<ConfigEntryBase>();
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
            // toSend.Add(SizeDifference.sizeOption);
            return toSend;
        }
        #endregion
        void Awake()
        {
            CustomLogger = BepInEx.Logging.Logger.CreateLogSource("Emergency Dice Updated");
            lethalThingsAssembly = GetAssembly("evaisa.lethalthings");
            lethalThingsPresent = IsModPresent("evaisa.lethalthings", "LethalThings compatibility enabled!");
            LethalMonAssembly = GetAssembly("LethalMon"); //This was before I learned about soft dependencies lol
            LethalMonPresent = IsModPresent("LethalMon", "LethalMon compatibility enabled!");
            LCOfficeAssembly = GetAssembly("Piggy.LCOffice"); //This was before I learned about soft dependencies lol
            LCOfficePresent = IsModPresent("Piggy.LCOffice", "LCOffice compatibility enabled!");
            SurfacedPresent = IsModPresent("Surfaced", "Surfaced compatibility enabled!");
            LCTarotCardPresent = IsModPresent("LCTarotCard", "LCTarotCard compatibility enabled!");
            TakeyPlushPresent = IsModPresent("com.github.zehsteam.TakeyPlush", "TakeyPlush compatibility enabled!");
            CodeRebirthPresent = IsModPresent("CodeRebirth", "CodeRebirth compatibility enabled!");
            DiversityPresent = IsModPresent("Chaos.Diversity", "Diversity: Remastered compatibility enabled!");
            BombCollarPresent = IsModPresent("Jordo.BombCollar", "Bomb Collar compatibility enabled! >:)");
            //MoreCompanyPresent = IsModPresent("me.swipez.melonloader.morecompany", "MoreCompany compatibility enabled!");
            NavMeshInCompanyPresent = IsModPresent("dev.kittenji.NavMeshInCompany", "Nav Mesh In Company compatibility enabled! >:)");
            NightOfTheLivingMimicPresent = IsModPresent("Slayer6409.NightOfTheLivingMimic", ">:)");
            TooManyEmotesPresent = IsModPresent("FlipMods.TooManyEmotes", "Dancing Enabled!");
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
                LethalConfigPresent = true;
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Emergency Dice.cfg"),true);
            ModConfig();
            InvisibleEnemy.Config();
            // SizeDifference.Config();
            BlameGlitch.Config();
            AlarmCurse.Config();
            if (SurfacedPresent)
            {
                Flinger.Config();
            }
            
            customCfg = new CustomConfigs(BepInExConfig);
            customCfg.GenerateConfigs(CustomEnemyEventCount.Value, CustomItemEventCount.Value, CustomTrapEventCount.Value);
            DieBehaviour.Config();

            if (superDebugMode.Value) db(); 

            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice"));
            LoadedAssets2 = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mysterydice2"));
            
            sounds.Add("MineDetonate", LoadedAssets.LoadAsset<AudioClip>("MineDetonate"));
            sounds.Add("MineTrigger", LoadedAssets.LoadAsset<AudioClip>("MineTrigger"));
            sounds.Add("Bell2", LoadedAssets.LoadAsset<AudioClip>("Bell2"));
            sounds.Add("Bad1", LoadedAssets.LoadAsset<AudioClip>("Bad1"));
            sounds.Add("Good2", LoadedAssets.LoadAsset<AudioClip>("Good2"));
            sounds.Add("glitch", LoadedAssets.LoadAsset<AudioClip>("glitch"));
            sounds.Add("purr", LoadedAssets.LoadAsset<AudioClip>("purr"));
            sounds.Add("alarmcurse", LoadedAssets.LoadAsset<AudioClip>("alarmcurse"));
            sounds.Add("Meeting_Sound", LoadedAssets2.LoadAsset<AudioClip>("Meeting_Sound"));
            sounds.Add("Dawg", LoadedAssets2.LoadAsset<AudioClip>("Dawg"));
            sounds.Add("Jaws", LoadedAssets2.LoadAsset<AudioClip>("Jaws"));
            sounds.Add("FireAlarm", LoadedAssets2.LoadAsset<AudioClip>("FireAlarm"));
            sounds.Add("Paparazzi", LoadedAssets2.LoadAsset<AudioClip>("Paparazzi"));
            sounds.Add("WindowsError", LoadedAssets2.LoadAsset<AudioClip>("WindowsError"));
            sounds.Add("disconnect", LoadedAssets2.LoadAsset<AudioClip>("disconnect"));
            sounds.Add("DoorLeft", LoadedAssets2.LoadAsset<AudioClip>("DoorLeft"));
            sounds.Add("DoorRight", LoadedAssets2.LoadAsset<AudioClip>("DoorRight"));
            sounds.Add("AudioTest", LoadedAssets2.LoadAsset<AudioClip>("AudioTest"));
            sounds.Add("aot", LoadedAssets2.LoadAsset<AudioClip>("aot"));
            sounds.Add("wiwiwi", LoadedAssets2.LoadAsset<AudioClip>("wiwiwi"));
            sounds.Add("BANANA", LoadedAssets2.LoadAsset<AudioClip>("BANANA"));
            sounds.Add("tuturu", LoadedAssets2.LoadAsset<AudioClip>("tuturu"));
            sounds.Add("mah-boi", LoadedAssets2.LoadAsset<AudioClip>("mah-boi"));
            sounds.Add("Bad Romance", LoadedAssets2.LoadAsset<AudioClip>("bad"));
            sounds.Add("Boom", LoadedAssets2.LoadAsset<AudioClip>("boom"));
            sounds.Add("Bald", LoadedAssets2.LoadAsset<AudioClip>("Glitchimnotbald"));
            sounds.Add("BerthaCollide", LoadedAssets2.LoadAsset<AudioClip>("BerthaCollide"));
            sounds.Add("Jimothy", LoadedAssets2.LoadAsset<AudioClip>("JimHonk"));
            sounds.Add("Beartrap", LoadedAssets2.LoadAsset<AudioClip>("BearTrapSnapOnPlayer"));
            sounds.Add("JanitorBald", LoadedAssets2.LoadAsset<AudioClip>("JanitorIdleSpeak12"));
            sounds.Add("Duck", LoadedAssets2.LoadAsset<AudioClip>("DuckSpawn"));
            sounds.Add("Fumo", LoadedAssets2.LoadAsset<AudioClip>("Fumo"));
            sounds.Add("Steve", LoadedAssets2.LoadAsset<AudioClip>("Steve"));
            sounds.Add("Yeehaw", LoadedAssets2.LoadAsset<AudioClip>("Yeehaw"));
            sounds.Add("KeepDice", LoadedAssets2.LoadAsset<AudioClip>("KeepDice"));
            sounds.Add("NancyHair", LoadedAssets2.LoadAsset<AudioClip>("NancySorryGlitch"));
            sounds.Add("Lizard", LoadedAssets2.LoadAsset<AudioClip>("lizard"));
            
            WarningBracken = LoadedAssets.LoadAsset<Sprite>("bracken");
            WarningJester = LoadedAssets.LoadAsset<Sprite>("jester");
            WarningDeath = LoadedAssets.LoadAsset<Sprite>("death");
            WarningLuck = LoadedAssets.LoadAsset<Sprite>("luck");
            
            jobApplication = LoadedAssets2.LoadAsset<Material>("JobApplication");
            angyGlitch = LoadedAssets2.LoadAsset<Material>("AngyGlitch");
            
            DiceGal = LoadedAssets2.LoadAsset<GameObject>("DiceGal"); 
            diceGalUnlockable = LoadedAssets2.LoadAsset<UnlockableItemDef>("DiceGalUnlockable");
            
            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.name = "DiceNetworker";
            NetworkerPrefab.AddComponent<Networker>();
            
            AgentObjectPrefab = LoadedAssets2.LoadAsset<GameObject>("AgentObject");
            if (aprilFoolsConfig.Value) AgentObjectPrefab.GetComponent<NavMeshAgent>().speed = 9;

            DebugMenuPrefab = LoadedAssets2.LoadAsset<GameObject>("DebugMenu");
            NewSelectMenuPrefab = LoadedAssets2.LoadAsset<GameObject>("NewSelectMenu");
            DebugMenuButtonPrefab = LoadedAssets2.LoadAsset<GameObject>("DebugButton");
            DebugSubButtonPrefab = LoadedAssets2.LoadAsset<GameObject>("SubmenuButton");
            
            
            PlayerNodeController = LoadedAssets2.LoadAsset<GameObject>("PlayerNodeController");

            JumpscareCanvasPrefab = LoadedAssets2.LoadAsset<GameObject>("JumpscareCanvas");
            JumpscareCanvasPrefab.AddComponent<Jumpscare>();
            SpiderCanvasPrefab = LoadedAssets2.LoadAsset<GameObject>("SpiderCanvas");
            SpiderMoverPrefab = LoadedAssets2.LoadAsset<GameObject>("SpiderMover");
            
            PathfinderPrefab = LoadedAssets.LoadAsset<GameObject>("Pathfinder");
            PathfinderPrefab.AddComponent<Pathfinder.PathfindBehaviour>();
            
            PathfinderSpawner = LoadedAssets.LoadAsset<Item>("Pathblob");

            Pathfinder.BlobspawnerBehaviour scriptBlobspawner = PathfinderSpawner.spawnPrefab.AddComponent<Pathfinder.BlobspawnerBehaviour>();
            scriptBlobspawner.grabbable = true;
            scriptBlobspawner.grabbableToEnemies = true;
            scriptBlobspawner.itemProperties = PathfinderSpawner;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(NetworkerPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(AgentObjectPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderSpawner.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(PathfinderPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MysteryDice.DiceGal);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(diceGalUnlockable.unlockable.prefabObject);
            
            if(diceGalUnlockable == null) CustomLogger.LogError("DiceGalUnlockable is null!"); 
            if(!DisableGal.Value) LethalLib.Modules.Unlockables.RegisterUnlockable(MysteryDice.diceGalUnlockable, GalPrice.Value, StoreType.ShipUpgrade);

            LoadDice();
            
            Keybinds  = new IngameKeybinds();
            Keybinds.DebugMenu.performed += context => DebugMenu();

            harmony.PatchAll();
            //All config edits come before this
            if (LethalConfigPresent) ConfigManager.setupLethalConfig();

            CustomLogger.LogInfo("The Emergency Dice mod was initialized!");
        }
        public static void RegisterNewEffect(IEffect effect, bool defaultOff = false, bool superDebug = false)
        {
            if (superDebug)
            {
                DieBehaviour.CompleteEffects.Add(effect);
            }
            else
            {
                DieBehaviour.AllEffects.Add(effect);
                DieBehaviour.CompleteEffects.Add(effect);
                
                ConfigEntry<bool> cfg;
                ConfigEntry<bool> fav;
                if (defaultOff) 
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Module Effects",
                        effect.Name,
                        false,
                        effect.Tooltip);
                }
                else  
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Module Effects",
                        effect.Name,
                        true,
                        effect.Tooltip);
                }
                fav = MysteryDice.BepInExConfig.Bind<bool>("Favorites", effect.Name, false, effect.Tooltip);
                DieBehaviour.effectConfigs.Add(cfg);
                //DieBehaviour.favConfigs.Add(fav);
                if (cfg.Value)
                    DieBehaviour.AllowedEffects.Add(effect);
                switch (effect.Outcome)
                {
                    case EffectType.Awful:
                        DieBehaviour.AwfulEffects.Add(effect);
                        break;
                    case EffectType.Bad:
                        DieBehaviour.BadEffects.Add(effect);
                        break;
                    case EffectType.Mixed:
                        DieBehaviour.MixedEffects.Add(effect);
                        break;
                    case EffectType.Good:
                        DieBehaviour.GoodEffects.Add(effect);
                        break;
                    case EffectType.Great:
                        DieBehaviour.GreatEffects.Add(effect);
                        break;
                }
                
                if (LethalConfigPresent) ConfigManager.addConfig(cfg);
            }
            DieBehaviour.AllEffects = DieBehaviour.AllEffects.OrderBy(o => o.Name).ToList();
            DieBehaviour.CompleteEffects = DieBehaviour.CompleteEffects.OrderBy(o => o.Name).ToList();
        }

        
        internal static void MainRegisterNewEffect(IEffect effect,bool defaultOff = false, bool superDebug = false)
        {
            if (superDebug)
            {
                DieBehaviour.CompleteEffects.Add(effect);
            }
            else
            {
                DieBehaviour.AllEffects.Add(effect);
                ConfigEntry<bool> cfg;
                if (defaultOff)
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Allowed Effects",
                        effect.Name,
                        false,
                        effect.Tooltip);
                }
                else
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Allowed Effects",
                        effect.Name,
                        true,
                        effect.Tooltip);
                }
                var fav = MysteryDice.BepInExConfig.Bind<bool>("Favorites", effect.Name, false, effect.Tooltip);

               
                DieBehaviour.effectConfigs.Add(cfg);
                //DieBehaviour.favConfigs.Add(fav);
                if (cfg.Value)
                {
                    if (effect is GalEffect galEffect)
                    {
                        DiceGalAI.GalEffects.Add(galEffect);
                        if(DisableGal.Value) DieBehaviour.AllowedEffects.Add(galEffect);
                    }
                    else
                    {
                        DieBehaviour.AllowedEffects.Add(effect);
                    }
                    
                    switch (effect.Outcome)
                    {
                        case EffectType.Awful:
                            DieBehaviour.AwfulEffects.Add(effect);
                            break;
                        case EffectType.Bad:
                            DieBehaviour.BadEffects.Add(effect);
                            break;
                        case EffectType.Mixed:
                            DieBehaviour.MixedEffects.Add(effect);
                            break;
                        case EffectType.Good:
                            DieBehaviour.GoodEffects.Add(effect);
                            break;
                        case EffectType.Great:
                            DieBehaviour.GreatEffects.Add(effect);
                            break;
                    }
                }
            }
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
            TryRequestAdmin();
            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            bool isSlayer = localPlayer.playerSteamId == slayerSteamID;
            bool isHost = GameNetworkManager.Instance.localPlayerController.IsHost;
            bool hasDebugAccess = Networker.Instance != null && canOpenAdminMenu();
            bool debugModeEnabled = superDebugMode.Value || isSlayer;
            
            if (!debugButton.Value && !isSlayer) return;
            if (hasDebugAccess)
            {
                if(isSlayer) DebugMenuStuff.showDebugMenu(true, true, true);
                else if(isHost) DebugMenuStuff.showDebugMenu(BetterDebugMenu.Value, debugModeEnabled, true);
                else DebugMenuStuff.showDebugMenu(BetterDebugMenu.Value, false, isHost);
            }
        }

        public static bool canOpenAdminMenu()
        {
            var player = StartOfRound.Instance.localPlayerController;
            ulong steamId = player.playerSteamId;

            return steamId == slayerSteamID
                   || player.IsHost
                   || isAdmin;
        }
        public static void TryRequestAdmin()
        {
            if (MysteryDice.triedRequestingAdmin || MysteryDice.isAdmin)
                return;

            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer != null)
            {
                MysteryDice.triedRequestingAdmin = true;
                var steamId = localPlayer.playerSteamId;
                Networker.Instance.RequestAdminStateServerRpc(steamId);
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
            if (isPresent) MysteryDice.CustomLogger.LogMessage(logMessage);
            return isPresent;
        }
        
        public static void ExtendedLogging(string logMessage, LogLevel level = LogLevel.Info)
        {
            if(DebugLogging.Value) CustomLogger.Log(level, logMessage);
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
            
            DieSteve = LoadedAssets2.LoadAsset<Item>("stevedieitem");
            DieSteve.canBeGrabbedBeforeGameStart = true;
            SteveLeDice scriptDieSteve = DieSteve.spawnPrefab.GetComponent<SteveLeDice>();
            scriptDieSteve.myType = DieBehaviour.DiceType.STEVE;
            scriptDieSteve.grabbable = true;
            scriptDieSteve.grabbableToEnemies = true;
            scriptDieSteve.itemProperties = DieSteve;
            RegisteredDice.Add(DieSteve);
            
            DieCodeRebirth = LoadedAssets2.LoadAsset<Item>("coderebirthdieitem");
            DieCodeRebirth.minValue = 150;
            DieCodeRebirth.maxValue = 210;
            DieCodeRebirth.canBeGrabbedBeforeGameStart = true;
            CodeRebirthDie scriptCodeRebirth = DieCodeRebirth.spawnPrefab.AddComponent<CodeRebirthDie>();
            scriptCodeRebirth.myType = DieBehaviour.DiceType.CODEREBIRTH;
            scriptCodeRebirth.grabbable = true;
            scriptCodeRebirth.grabbableToEnemies = true;
            scriptCodeRebirth.itemProperties = DieCodeRebirth;
            RegisteredDice.Add(DieCodeRebirth);
            
            ///
            DieSurfaced = LoadedAssets2.LoadAsset<Item>("surfaceddieitem");
            DieSurfaced.minValue = 150;
            DieSurfaced.maxValue = 210;
            DieSurfaced.canBeGrabbedBeforeGameStart = true;
            SurfacedDie scriptSurfaced = DieSurfaced.spawnPrefab.AddComponent<SurfacedDie>();
            scriptSurfaced.myType = DieBehaviour.DiceType.SURFACED;
            scriptSurfaced.grabbable = true;
            scriptSurfaced.grabbableToEnemies = true;
            scriptSurfaced.itemProperties = DieSurfaced;
            RegisteredDice.Add(DieSurfaced);
            
            ///
            
            DieEmergency = LoadedAssets.LoadAsset<Item>("Emergency Dice Script");
            DieEmergency.highestSalePercentage = 80;

            DieEmergency.canBeGrabbedBeforeGameStart = true;
            EmergencyDie scriptEmergency = DieEmergency.spawnPrefab.AddComponent<EmergencyDie>();
            scriptEmergency.myType = DieBehaviour.DiceType.EMERGENCY;
            scriptEmergency.grabbable = true;
            scriptEmergency.grabbableToEnemies = true;
            scriptEmergency.itemProperties = DieEmergency;

            RegisteredDice.Add(DieEmergency);

            ///

            DieChronos = LoadedAssets.LoadAsset<Item>("Chronos");
            DieChronos.minValue = 120;
            DieChronos.maxValue = 140;
            DieChronos.canBeGrabbedBeforeGameStart = true;
            
            ChronosDie scriptChronos = DieChronos.spawnPrefab.AddComponent<ChronosDie>();
            scriptChronos.myType = DieBehaviour.DiceType.CHRONOS;
            scriptChronos.grabbable = true;
            scriptChronos.grabbableToEnemies = true;
            scriptChronos.itemProperties = DieChronos;

            RegisteredDice.Add(DieChronos);

            ///

            DieGambler = LoadedAssets2.LoadAsset<Item>("GamblerItem");

            DieGambler.minValue = 100;
            DieGambler.maxValue = 130;
            DieGambler.canBeGrabbedBeforeGameStart = true;
            
            GamblerDie scriptGambler = DieGambler.spawnPrefab.AddComponent<GamblerDie>();
            scriptGambler.myType = DieBehaviour.DiceType.GAMBLER;
            scriptGambler.grabbable = true;
            scriptGambler.grabbableToEnemies = true;
            scriptGambler.itemProperties = DieGambler;
            RegisteredDice.Add(DieGambler);
            
            ///

            DieSacrificer = LoadedAssets2.LoadAsset<Item>("SacrificerItem");
            DieSacrificer.minValue = 170;
            DieSacrificer.maxValue = 230;
            DieSacrificer.canBeGrabbedBeforeGameStart = true;
            
            SacrificerDie scriptSacrificer = DieSacrificer.spawnPrefab.AddComponent<SacrificerDie>();
            scriptSacrificer.myType = DieBehaviour.DiceType.SACRIFICER;
            scriptSacrificer.grabbable = true;
            scriptSacrificer.grabbableToEnemies = true;
            scriptSacrificer.itemProperties = DieSacrificer;

            RegisteredDice.Add(DieSacrificer);

            ///

            DieSaint = LoadedAssets.LoadAsset<Item>("Saint");
            DieSaint.minValue = 210;
            DieSaint.maxValue = 280;
            DieSaint.canBeGrabbedBeforeGameStart = true;

            SaintDie scriptSaint = DieSaint.spawnPrefab.AddComponent<SaintDie>();
            scriptSaint.myType = DieBehaviour.DiceType.SAINT;
            scriptSaint.grabbable = true;
            scriptSaint.grabbableToEnemies = true;
            scriptSaint.itemProperties = DieSaint;

            RegisteredDice.Add(DieSaint);

            ///

            DieRusty = LoadedAssets2.LoadAsset<Item>("RustyItem");
            DieRusty.minValue = 90;
            DieRusty.maxValue = 160;
            DieRusty.canBeGrabbedBeforeGameStart = true;

            RustyDie scriptRusty = DieRusty.spawnPrefab.AddComponent<RustyDie>();
            scriptRusty.myType = DieBehaviour.DiceType.RUSTY;
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
                { (DieSteve.itemName, Consts.Default), 25 },
                { (DieSteve.itemName, Consts.Experimentation), 16 },
                { (DieSteve.itemName, Consts.Assurance), 26 },
                { (DieSteve.itemName, Consts.Vow), 25 },
                { (DieSteve.itemName, Consts.Offense), 17 },
                { (DieSteve.itemName, Consts.March), 27 },
                { (DieSteve.itemName, Consts.Rend), 26 },
                { (DieSteve.itemName, Consts.Dine), 36 },
                { (DieSteve.itemName, Consts.Titan), 25 },
                { (DieSteve.itemName, Consts.Adamance), 15 },
                { (DieSteve.itemName, Consts.Artifice), 28 },
                { (DieSteve.itemName, Consts.Embrion), 45 },
                { (DieCodeRebirth.itemName, Consts.Default), 25 },
                { (DieCodeRebirth.itemName, Consts.Experimentation), 16 },
                { (DieCodeRebirth.itemName, Consts.Assurance), 26 },
                { (DieCodeRebirth.itemName, Consts.Vow), 25 },
                { (DieCodeRebirth.itemName, Consts.Offense), 17 },
                { (DieCodeRebirth.itemName, Consts.March), 27 },
                { (DieCodeRebirth.itemName, Consts.Rend), 26 },
                { (DieCodeRebirth.itemName, Consts.Dine), 36 },
                { (DieCodeRebirth.itemName, Consts.Titan), 25 },
                { (DieCodeRebirth.itemName, Consts.Adamance), 15 },
                { (DieCodeRebirth.itemName, Consts.Artifice), 28 },
                { (DieCodeRebirth.itemName, Consts.Embrion), 45 },
                { (DieSurfaced.itemName, Consts.Default), 25 },
                { (DieSurfaced.itemName, Consts.Experimentation), 16 },
                { (DieSurfaced.itemName, Consts.Assurance), 26 },
                { (DieSurfaced.itemName, Consts.Vow), 25 },
                { (DieSurfaced.itemName, Consts.Offense), 17 },
                { (DieSurfaced.itemName, Consts.March), 27 },
                { (DieSurfaced.itemName, Consts.Rend), 26 },
                { (DieSurfaced.itemName, Consts.Dine), 36 },
                { (DieSurfaced.itemName, Consts.Titan), 25 },
                { (DieSurfaced.itemName, Consts.Adamance), 15 },
                { (DieSurfaced.itemName, Consts.Artifice), 28 },
                { (DieSurfaced.itemName, Consts.Embrion), 45 },
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
                DieEmergency.isScrap = true;
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
                if (die == DieEmergency && !DieEmergencyAsScrap.Value) continue;

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
                    int rateToBe = rate.Value;
                    if (aprilFoolsConfig.Value) rateToBe *= 2; 
                    Items.RegisterScrap(die, rateToBe, level.Value);
                }
                var defaltRateToBe = defaultRate.Value;
                if (aprilFoolsConfig.Value) defaltRateToBe *= 2; 
                Items.RegisterScrap(die, defaltRateToBe, Levels.LevelTypes.All);
            }
        }
    }
    
}
