using BepInEx.Configuration;
using GameNetcodeStuff;
using MysteryDice.Dice;
using MysteryDice.Effects;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dawn.Utils;
using MysteryDice.CompatThings;
using MysteryDice.Gal;
using MysteryDice.MiscStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using PlayerControllerBExtensions = MysteryDice.Extensions.PlayerControllerBExtensions;
using Random = UnityEngine.Random;
using SmartAgentNavigator = MysteryDice.MiscStuff.SmartAgentNavigator;

// ReSharper disable Unity.PerformanceAnalysis
namespace MysteryDice
{
    public class Networker : NetworkBehaviour
    {
        public static Networker Instance;
        internal static EntranceTeleport[] _entrancePoints = [];
        public static float RebelTimer = 0f;
        public static bool CoilheadIgnoreStares = false;
        public static List<UnlockableSuit> orderedSuits = new List<UnlockableSuit>();
        public List<AudioSource> AudioSources = new List<AudioSource>();
        public List<AudioSource> FreebirdAudioSources = new List<AudioSource>();
        public List<ulong> cursedPeople = new();
        public List<GameObject> playerScanNodes = new();
        public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;


        public void Awake()
        {
            StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;
            StartCoroutine(DelayStartup());
            foreach (string part in MysteryDice.cursedIDs.Value.Split(','))
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (ulong.TryParse(trimmed, out ulong result))
                    cursedPeople.Add(result);
            }
            
            
            if (IsServer) return;
            RequestSyncCursedServerRPC();
            DieBehaviour.AllowedEffects.Clear();
            StartCoroutine(SyncRequest());
            

        }

        public void DelaySuitGet()
        {
            orderedSuits = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>()
                .OrderBy(suit =>
                    suit.syncedSuitID.Value >= 0 &&
                    suit.syncedSuitID.Value < StartOfRound.Instance.unlockablesList.unlockables.Count
                        ? StartOfRound.Instance.unlockablesList.unlockables[suit.syncedSuitID.Value].unlockableName
                        : string.Empty) // Default case to prevent errors
                .ToList();
        }
        public IEnumerator DelayStartup()
        {
            yield return new WaitForSeconds(3.5f);
            DelaySuitGet();
            StartCoroutine(ManyAds.LoadRemoteTextLists());
            AddControllerToPlayers();
        }

        public void AddControllerToPlayers()
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if(PlayerControllerBExtensions.IsLocalPlayer(player)) AddControllerServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player));
                else if(Misc.IsPlayerReal(player))
                {
                    InstantiateSpecialPrefabForPlayer(player);
                }
            }
        }
        private void InstantiateSpecialPrefabForPlayer(PlayerControllerB player)
        {
            string topText = "", bottomText="";
            Color backgroundColor = Color.black, textColor = Color.black;
            
            GameObject prefab = null;

            switch (player.playerSteamId)
            {
                case 76561198298343090: // mel
                //case 76561198216220844: //Macho
                    topText  = "Piss Girl";
                    bottomText = "Likes Piss";
                    backgroundColor = new Color32(255, 255, 0, 255);
                    textColor = new Color32(152, 108, 0, 255);
                    break;
                case 9876561198984467725: // glitch
                    topText  = "Bald";
                    bottomText = "Shiny Head";
                    backgroundColor = new Color32(0, 255, 0, 255);
                    textColor = new Color32(0, 123, 0, 255);
                    break;
                case 76561198003293676: // beef
                    topText  = "Short";
                    bottomText = "1 mm tall";
                    backgroundColor = new Color32(255, 0, 0, 255);
                    textColor = new Color32(80, 0, 0, 255);
                    break;
                case 76561199092131418: // cross
                    topText  = "Stinky";
                    bottomText = "Eww Stinky";
                    backgroundColor = new Color32(0, 0, 255, 255);
                    textColor = new Color32(0, 255, 0, 255);
                    break;
                case 76561198077184650: //Me
                    topText  = "Dice Man";
                    bottomText = "Sacrificer Broken";
                    backgroundColor = new Color32(0, 0, 0, 255);
                    textColor = new Color32(0, 0, 0, 255);
                    break;
                case 76561198216220844: //Macho
                //case 76561198298343090: // mel
                    topText  = "Macho";
                    bottomText = "Macho Macho Macho";
                    backgroundColor = new Color32(150, 0, 255, 255);
                    textColor = new Color32(255, 0, 0, 255);
                    break;
                case 76561198399127090: //Xu
                    topText  = "Hi Mu!";
                    bottomText = " ";
                    backgroundColor = new Color32(236, 189, 196, 255);
                    textColor = new Color32(250, 138, 170, 255);
                    break;
                case 76561199094139351: //Lizzie
                    topText  = "Lizzie!";
                    bottomText = "Friends of the Blob";
                    backgroundColor = new Color32(236, 20, 196, 255);
                    textColor = new Color32(250, 180, 170, 255);
                    break;
                case 76561198164429786: //Rodrigo
                    topText  = "Boom Bird";
                    bottomText = "Go Scarab";
                    backgroundColor = new Color32(0, 255, 0, 255);
                    textColor = new Color32(0, 0, 0, 255);
                    break;
                case 76561198086086035: //Nut
                    topText  = "Suit Guy";
                    bottomText = "Makes Cool Suits";
                    backgroundColor = new Color32(136, 0, 180, 255);
                    textColor = new Color32(0, 0, 0, 255);
                    break;
                    
            }

            prefab = MysteryDice.PlayerNodeController;
            if (prefab != null && player.transform.Find("CustomPlayerScanNode") == null)
            {
                ForceScanColorOnItem fsn;
                ScanNodeProperties scnnode;
                if (topText == "") return;
                if (PlayerControllerBExtensions.IsLocalPlayer(player) && !MysteryDice.showOwnScanNode.Value) return;
                var instance = Instantiate(prefab, player.transform.position + new Vector3(0,2.5f,0), Quaternion.identity);
                var pt = instance.AddComponent<PlayerTracker>();
                pt.init(player, new Vector3(0,2.5f,0));
                fsn = instance.GetComponentInChildren<ForceScanColorOnItem>();
                fsn.borderColor = backgroundColor;
                fsn.textColor = textColor;
                scnnode = instance.GetComponentInChildren<ScanNodeProperties>();
                scnnode.headerText = topText;
                scnnode.subText = bottomText;
                if (player.playerSteamId == 76561198077184650) instance.AddComponent<RainbowScanNode>();
                playerScanNodes.Add(instance);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddControllerServerRPC(int playerID)
        {
            AddControllerClientRPC(playerID);
        }

        [ClientRpc]
        public void AddControllerClientRPC(int playerID)
        {
            InstantiateSpecialPrefabForPlayer(StartOfRound.Instance.allPlayerScripts[playerID]);
        }
        
        //thanks xu
        private void OnNewRoundStart()
        {
            if (MysteryDice.CodeRebirthPresent) CodeRebirthCheckConfigs.listAll();
            _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
            foreach (EntranceTeleport? entrance in _entrancePoints)
            {
                if (!entrance.FindExitPoint())
                {
                    MysteryDice.CustomLogger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
                }
            }
            StartCoroutine(ManyAds.LoadRemoteTextLists());
        }
        public IEnumerator SyncRequest()
        {
            while (!GameNetworkManager.Instance.GetComponent<NetworkManager>().IsConnectedClient)
            {
                yield return new WaitForSeconds(0.5f);
            }

            while (GameNetworkManager.Instance.localPlayerController == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            RequestEffectConfigServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
            RequestConfigSyncServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        }

        public override void OnDestroy()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug($"Dice: [DestroyDebugger] {gameObject.name} was destroyed!\nEither you left the game or something bad happened!\nStackTrace:\n" + new StackTrace());
            base.OnDestroy();
        }
        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug($"Dice: [Network] {gameObject.name} is despawning!\nEither you left the game or something bad happened! StackTrace:\n" + new StackTrace());
            StartOfRoundPatch.ResetSettingsShared();
            base.OnNetworkDespawn();
        }

        void FixedUpdate()
        {
            UpdateMineTimers();
            if (Armageddon.IsEnabled) Armageddon.BoomTimer();
            HyperShake.FixedUpdate();
            LeverShake.FixedUpdate();
            Drunk.FixedUpdate();
            Lizard.FixedUpdate();
            ChangePlaces.FixedUpdate();
            TwitchSpawner();
            if (SelectEffect.ReviveOpen && SelectEffect.EffectMenu == null) SelectEffect.ReviveOpen = false;
        }

        void Update()
        {
            ModifyPitch.PitchFluctuate();
            RebelCoilheads();
            AlarmCurse.TimerUpdate();
        }

        
        private Queue<(IEffect effect, string WhoRolled)> spawnQueue = new Queue<(IEffect, string)>();

        private bool twitchCanRoll = true;

        public void TwitchSpawner()
        {
            if (StartOfRound.Instance != null && !StartOfRound.Instance.inShipPhase &&
                StartOfRound.Instance.shipHasLanded)
            {
                twitchCanRoll = true;
            }
            else twitchCanRoll = false;

            if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion") twitchCanRoll = false;
            if (StartOfRound.Instance.currentLevel.PlanetName == "98 Galetry") twitchCanRoll = false;
            if (StartOfRound.Instance.currentLevel.sceneName == "Oxyde") twitchCanRoll = false;

            if (IsServer && twitchCanRoll && spawnQueue.Count > 0) ProcessSpawnQueue();

        }

        public int CheckScaling()
        {
            Terminal terminal = FindFirstObjectByType<Terminal>();
            if (terminal == null) return 0;
            var scrapInShip = StartOfRound.Instance.GetValueOfAllScrap(true);
            var inARow = StartOfRound.Instance.daysPlayersSurvivedInARow;
            var currentDay = StartOfRound.Instance.gameStats.daysSpent;
            switch (MysteryDice.brutalScaleType.Value)
            {
                case 0:
                    return (int)Scale(currentDay, 0, 50, MysteryDice.brutalStartingScale.Value,
                        MysteryDice.brutalMaxScale.Value);
                case 1:
                    return (int)Scale(scrapInShip, 0, 2500, MysteryDice.brutalStartingScale.Value,
                        MysteryDice.brutalMaxScale.Value);
                case 2:
                    return (int)Scale(inARow, 0, 15, MysteryDice.brutalStartingScale.Value,
                        MysteryDice.brutalMaxScale.Value);
                case 3:
                    var normalizedDay = currentDay / 50.0;
                    var normalizedScrap = scrapInShip / 2500.0;
                    var combinedNormalized = (normalizedDay + normalizedScrap) / 2.0;
                    var scaledValue = MysteryDice.brutalStartingScale.Value + combinedNormalized *
                        (MysteryDice.brutalMaxScale.Value - MysteryDice.brutalStartingScale.Value);
                    return (int)scaledValue;
                case 4:
                    return MysteryDice.brutalStartingScale.Value;
            }

            return 0;
        }

       
        public static double Scale(double x, double minInput, double maxInput, double minOutput, double maxOutput)
        {
            return minOutput + ((x - minInput) * (maxOutput - minOutput)) / (maxInput - minInput);
        }

        private int maxSpawnsPerFrame = 1;

        private void ProcessSpawnQueue()
        {
            int rollCount = 0;
            while (spawnQueue.Count > 0 && twitchCanRoll && rollCount < maxSpawnsPerFrame)
            {
                var (effect, who) = spawnQueue.Dequeue();
                StartCoroutine(RollEffects(effect, who));
                rollCount++;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void LogEffectsToOwnerServerRPC(string playerName, string effectName, int roll)
        {
            var parsedEffect = MysteryDice.chatDebug.None;
            Enum.TryParse<MysteryDice.chatDebug>(MysteryDice.debugChat.Value, out parsedEffect);
            if (MysteryDice.debugDice.Value)
                MysteryDice.CustomLogger.LogInfo($"[Debug] {playerName} rolled a {roll}: {effectName}");
            if (playerName == "Brutal" && MysteryDice.BrutalChat.Value)
            {
                LogEffectsToEveryoneClientRPC(playerName, effectName, roll);
                return;
            }
            if (playerName == "Brutal" && !StartOfRound.Instance.allPlayerScripts.Any(x => x.playerUsername == "Brutal")) return;
            if (parsedEffect == MysteryDice.chatDebug.Host) LogEffectsToHostClientRPC(playerName, effectName, roll);
            if (parsedEffect == MysteryDice.chatDebug.Everyone)
                LogEffectsToEveryoneClientRPC(playerName, effectName, roll);
            if (parsedEffect == MysteryDice.chatDebug.None) LogEffectsToSlayerClientRPC(playerName, effectName, roll);
        }

        [ClientRpc]
        public void LogEffectsToHostClientRPC(string playerName, string effectName, int roll)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsHost ||
                GameNetworkManager.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID)
                Misc.ChatWrite($"{playerName} rolled a {roll}: {effectName}");
        }

        [ClientRpc]
        public void LogEffectsToSlayerClientRPC(string playerName, string effectName, int roll)
        {
            if (GameNetworkManager.Instance.localPlayerController.playerSteamId == MysteryDice.slayerSteamID)
                Misc.ChatWrite($"{playerName} rolled a {roll}: {effectName}");
        }

        [ClientRpc]
        public void LogEffectsToEveryoneClientRPC(string playerName, string effectName, int roll)
        {
            Misc.ChatWrite($"{playerName} rolled a {roll}: {effectName}");
        }

        #region Config stuff

        [ServerRpc(RequireOwnership = false)]
        public void RequestEffectConfigServerRPC(int playerID)
        {
            foreach (var effect in DieBehaviour.AllowedEffects)
                SendConfigClientRPC(playerID, effect.Name);
        }

        [ClientRpc]
        public void SendConfigClientRPC(int playerID, string effectName)
        {
            var player = Misc.GetPlayerByUserID(playerID);
            if (player == null || !Misc.IsPlayerReal(player))
            {
                MysteryDice.CustomLogger.LogError($"Player with ID {playerID} is either null or not real.");
                return;
            }

            if (IsServer) return;
            if (Misc.isPlayerLocal(playerID))
            {
                DieBehaviour.AllowedEffects.Add(
                    DieBehaviour.AllEffects.Where(x => x.Name == effectName).First()
                );
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestConfigSyncServerRPC(int playerID)
        {
            List<ConfigEntryBase> configEntries = MysteryDice.GetListConfigs();
            foreach (var configEntry in configEntries)
            {
                // Convert the config entry to basic types that can be sent over the network
                string key = configEntry.Definition.Key;
                string section = configEntry.Definition.Section;
                int type = (int)Type.GetTypeCode(configEntry.SettingType);

                // Handle different types of config values (int, bool, string, etc.)
                if (configEntry.BoxedValue is int intValue)
                {
                    SendConfigsClientRPC(playerID, key, section, type, intValue);
                }
                else if (configEntry.BoxedValue is bool boolValue)
                {
                    SendConfigsClientRPC(playerID, key, section, type, bval: boolValue);
                }
                else if (configEntry.BoxedValue is string stringValue)
                {
                    SendConfigsClientRPC(playerID, key, section, type, sval: stringValue);
                }
                else if (configEntry.BoxedValue is Enum enumValue)
                {
                    SendConfigsClientRPC(playerID, key, section, type, enumVal: enumValue.ToString());
                }
            }
        }

        [ClientRpc]
        public void SendConfigsClientRPC(int playerID, string key, string section, int type, int ival = 0,
            bool bval = false, string sval = "", string enumVal = "")
        {
            if (IsServer || !Misc.isPlayerLocal(playerID)) return;
            try
            {
                List<ConfigEntryBase> configEntries = MysteryDice.GetListConfigs();
                foreach (var configEntry in configEntries)
                {
                    if (configEntry.Definition.Key == key && configEntry.Definition.Section == section)
                    {
                        switch ((TypeCode)type)
                        {
                            case TypeCode.Int32:
                                configEntry.BoxedValue = ival;
                                break;
                            case TypeCode.Boolean:
                                configEntry.BoxedValue = bval;
                                break;
                            case TypeCode.String:
                                configEntry.BoxedValue = sval;
                                break;
                            case TypeCode.Object:
                                if (configEntry.SettingType.IsEnum)
                                {
                                    configEntry.BoxedValue = Enum.Parse(configEntry.SettingType, enumVal);
                                }

                                break;
                        }
                    }
                }
            }
            catch
            {

            }

        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void RequestSyncCursedServerRPC()
        {
            SyncCursedPlayersClientRpc(cursedPeople.ToArray());
        }
        
        [ClientRpc]
        public void SyncCursedPlayersClientRpc(ulong[] cursedIds)
        {
            if (IsHost) return;
        
            cursedPeople.Clear();
            foreach (var id in cursedIds)
            {
                cursedPeople.Add(id);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Added "+id+" to cursed players.");
            }
        }
        
        

        // [ServerRpc(RequireOwnership = false)]
        // public void MoveTrapServerRpc(int getInstanceID, Vector3 targetPosition, bool isInside)
        // {
        //     MoveTrapClientRpc(getInstanceID, targetPosition, isInside);
        // }
        //
        // [ClientRpc]
        // public void MoveTrapClientRpc(int getInstanceID, Vector3 targetPosition, bool isInside)
        // {
        //     SmartAgentNavigator agent = PlayerControllerBPatch.smartAgentNavigators.Find(x => x.GetInstanceID() == getInstanceID);
        //     if (agent == null)
        //     {
        //         Debug.LogError($"No SmartAgent found for instance ID {getInstanceID}. Total smartAgents: {PlayerControllerBPatch.smartAgentNavigators.Count}");
        //         return;
        //     }
        //     Debug.LogWarning($"Trying to move trap to {targetPosition} and inside is {isInside}");
        //     agent.DoPathingToDestination(targetPosition, isInside, false, null);
        // }
        [ServerRpc(RequireOwnership = false)]
        public void MoveTrapServerRpc(int getInstanceID, Vector3 targetPosition, bool isInside)
        {
            SmartAgentNavigator agent =
                PlayerControllerBPatch.smartAgentNavigators.Find(x => x.GetInstanceID() == getInstanceID);
            if (agent == null)
            {
                //MysteryDice.CustomLogger.LogError($"No SmartAgent found for instance ID {getInstanceID}. Total smartAgents: {PlayerControllerBPatch.smartAgentNavigators.Count}");
                return;
            }

            //MysteryDice.CustomLogger.LogWarning($"Trying to move trap to {targetPosition} and inside is {isInside}");
            agent.DoPathingToDestination(targetPosition);
        }

        #region TwitchNetworking

        public IEnumerator RollEffects(IEffect effect, string who)
        {
            yield return new WaitForSeconds(Random.Range(5f, 12f));
            if (!twitchCanRoll) spawnQueue.Enqueue((effect, who));
            else
            {
                LogEffectsToOwnerServerRPC(who, effect.Name, 0);
                rollEffectRandomPlayerServerRPC(effect.Name);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void rollEffectRandomPlayerServerRPC(string effect)
        {
            var playerToBe = Misc.GetRandomPlayer();
            ProcessRollEffectClientRPC(effect, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,playerToBe));
        }

        [ClientRpc]
        public void ProcessRollEffectClientRPC(string effect, int playerID)
        {
            if(!Misc.isPlayerLocal(playerID)) return;
            
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling from queue");
            DieBehaviour.AllowedEffects.Find(x => x.Name == effect).Use();
        }

        [ServerRpc(RequireOwnership = false)]
        public void QueueRandomDiceEffectServerRPC(string who)
        {
            spawnQueue.Enqueue((DieBehaviour.AllowedEffects[Random.Range(0, DieBehaviour.AllowedEffects.Count)], who));
        }

        [ServerRpc(RequireOwnership = false)]
        public void QueueSpecificDiceEffectServerRPC(string who, string effectName)
        {
            spawnQueue.Enqueue((DieBehaviour.AllowedEffects.Find(x => x.Name == effectName), who));
        }

        [ServerRpc(RequireOwnership = false)]
        public void QueueSelectTypeServerRPC(EffectType effectType, string who)
        {
            List<IEffect> effectsToUse = new List<IEffect>();
            switch (effectType)
            {
                case EffectType.Awful:
                    effectsToUse.AddRange(DieBehaviour.AwfulEffects);
                    break;
                case EffectType.Bad:
                    effectsToUse.AddRange(DieBehaviour.BadEffects);
                    break;
                case EffectType.Good:
                    effectsToUse.AddRange(DieBehaviour.GoodEffects);
                    break;
                case EffectType.Great:
                    effectsToUse.AddRange(DieBehaviour.GreatEffects);
                    break;
                case EffectType.Mixed:
                    effectsToUse.AddRange(DieBehaviour.MixedEffects);
                    break;
            }

            spawnQueue.Enqueue((effectsToUse[Random.Range(0, effectsToUse.Count)], who));

        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void NemosplosionServerRPC()
        {
            int amount = 12;
            int radius = 2;
            var enemy = Misc.getEnemyByName("Nemo");
            var RM = RoundManager.Instance;
            if (enemy == null)
                return;
            var player = Misc.GetRandomAlivePlayer();
            List<EnemyAI> enemies = new List<EnemyAI>();
            for (int i = 0; i < amount; i++)
            {
                float angle = i * Mathf.PI * 2 / amount;
                Vector3 spawnPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    player.transform.position.y + 0.25f,
                    Mathf.Sin(angle) * radius
                );
                spawnPosition += player.transform.position;
                Vector3 directionToPlayer = player.transform.position - spawnPosition;
                Quaternion rotation = Quaternion.LookRotation(directionToPlayer);
                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    enemy.enemyType.enemyPrefab,
                    spawnPosition,
                    rotation);
                var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
                netObj.Spawn(destroyWithScene: true);
                var eai = enemyObject.GetComponent<EnemyAI>();
                RoundManager.Instance.SpawnedEnemies.Add(eai);
                enemies.Add(eai);
            }

            StartCoroutine(ExplodeNemos(enemies, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player)));
        }

        public IEnumerator ExplodeNemos(List<EnemyAI> enemies, int target)
        {
            yield return new WaitForSeconds(3f);
            StartOfRound.Instance.allowLocalPlayerDeath = false;
            foreach (var enemy in enemies)
            {
                Landmine.SpawnExplosion(enemy.transform.position, true, 1, 2, 50, 0, null, false);
                enemy.KillEnemy(false);
            }

            removeKillServerRPC(target, 7);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UnkillableServerRpc(int target)
        {
            removeKillClientRPC(target, 30);
        }

        [ServerRpc(RequireOwnership = false)]
        public void removeKillServerRPC(int target, int time)
        {
            removeKillClientRPC(target, time);
        }

        [ClientRpc]
        public void removeKillClientRPC(int target, int time)
        {
            if (!Misc.isPlayerLocal(target)) return;
            Instance.StartCoroutine(DoNoKill(time));
        }

        public IEnumerator DoNoKill(int time)
        {
            StartOfRound.Instance.allowLocalPlayerDeath = false;
            yield return new WaitForSeconds(time);
            StartOfRound.Instance.allowLocalPlayerDeath = true;
        }

        #region manyAds

        [ServerRpc(RequireOwnership = false)]
        public void AdServerRPC(bool item, string name, string top, string bottom)
        {
            AdClientRPC(item, name, top, bottom);
        }

        [ClientRpc]
        public void AdClientRPC(bool item, string name, string top, string bottom)
        {
            ManyAds.QueueAd(item, name, top, bottom);
        }
        [ServerRpc(RequireOwnership = false)]
        public void TriggerManyAdsServerRpc()
        {
            for (int i = 0; i < 4; i++)
                ManyAds.showRandomAd();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void TriggerSelectAdsServerRpc(int num)
        {
            for (int i = 0; i < num; i++)
                ManyAds.showRandomAd();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RandomAdItemServerRpc(string name)
        {
            ManyAds.QueueAd(true, name, ManyAds.getTopText(), ManyAds.getBottomText());
        }

        #endregion

        #region tooManyEmotes

        [ServerRpc(RequireOwnership = false)]
        public void RequestEmotesInRadiusServerRpc(Vector3 center, float radius)
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player.isPlayerDead || !player.isPlayerControlled)
                    continue;

                if (Vector3.Distance(player.transform.position, center) <= radius)
                {
                    PerformRandomEmoteClientRpc(player.playerClientId, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { player.playerClientId }
                        }
                    });
                }
            }
        } 
        [ClientRpc]
        private void PerformRandomEmoteClientRpc(ulong forClientId, ClientRpcParams rpcParams = default)
        {
            if (NetworkManager.Singleton.LocalClientId != forClientId) return;
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("FlipMods.TooManyEmotes")) return;

            PerformRandomEmote.PerformEmote();
        }
        
        #endregion

        #region SpawnSurrounded

        [ServerRpc(RequireOwnership = false)]
        public void SpawnSurroundedServerRPC(string enemyName, int amount = 10, int radius = 3, bool doSize = false, Vector3 size = default)
        {
            var enemy = Misc.getEnemyByName(enemyName);
            var RM = RoundManager.Instance;
            if (enemy == null)
                return;

            var player = Misc.GetRandomAlivePlayer();
            for (int i = 0; i < amount; i++)
            {
                float angle = i * Mathf.PI * 2 / amount;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Vector3 spawnPosition = player.transform.position + offset + new Vector3(0, 0.25f, 0);
                Quaternion rotation = Quaternion.LookRotation(player.transform.position - spawnPosition);

                RaycastHit hit;
                Vector3 rayOrigin = spawnPosition + Vector3.up * 2f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 10f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    spawnPosition = hit.point;
                }
                else
                {
                    continue;
                }

                if (spawnPosition.y > player.transform.position.y + 10f || spawnPosition.y < player.transform.position.y - 5f)
                    continue;

                GameObject enemyObject = UnityEngine.Object.Instantiate(enemy.enemyType.enemyPrefab, spawnPosition, rotation);
                var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
                netObj.Spawn(destroyWithScene: true);
                RM.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());

                if (doSize)
                {
                    setSizeClientRPC(netObj.NetworkObjectId, size);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnSurroundedTrapServerRPC(string trapName, int amount = 10, int radius = 3, bool doSize = false,
            Vector3 size = default)
        {
            var enemy = DynamicTrapEffect.getTrap(trapName);
            var RM = RoundManager.Instance;
            if (enemy == null)
                return;
            var player = Misc.GetRandomAlivePlayer();
            for (int i = 0; i < amount; i++)
            {
                float angle = i * Mathf.PI * 2 / amount;

                Vector3 horizontalOffset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                Vector3 baseSpawnPos = player.transform.position + horizontalOffset;

                Vector3 rayOrigin = baseSpawnPos + Vector3.up * 10f;

                Vector3 spawnPosition;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, LayerMask.GetMask("Default")))
                {
                    spawnPosition = hit.point + Vector3.up * 0.01f;
                }
                else
                {
                    spawnPosition = baseSpawnPos;
                }

                Vector3 directionToPlayer = player.transform.position - spawnPosition;
                Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    enemy.prefab,
                    spawnPosition,
                    rotation
                );

                var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
                if (netObj != null) netObj.Spawn(destroyWithScene: true);

                if (doSize)
                {
                    setSizeClientRPC(netObj.NetworkObjectId, size);
                }

                SceneManager.MoveGameObjectToScene(enemyObject, RoundManager.Instance.mapPropsContainer.scene);
            }
        }

        [ClientRpc]
        public void setSizeClientRPC(ulong objectId, Vector3 size, Quaternion rotation = default)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Vector3 newSize = new Vector3(obj.transform.localScale.x * size.x, obj.transform.localScale.y * size.y,
                    obj.transform.localScale.z * size.z);
                obj.transform.localScale = newSize;
                if (rotation != default) obj.transform.localRotation = rotation;
            }
        }

        [ClientRpc]
        public void MatchSizeClientRPC(ulong objectSource, ulong objectTarget, bool preserveRatio = true)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectSource, out var networkObj))
                return;

            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectTarget, out var networkObj2))
                return;

            GameObject source = networkObj.gameObject;
            GameObject target = networkObj2.gameObject;

            Renderer sourceRenderer = source.GetComponentInChildren<Renderer>();
            Renderer targetRenderer = target.GetComponentInChildren<Renderer>();

            if (sourceRenderer == null || targetRenderer == null)
            {
                MysteryDice.CustomLogger.LogWarning("One of the objects has no Renderer.");
                return;
            }

            Vector3 sourceSize = sourceRenderer.bounds.size;
            Vector3 targetSize = targetRenderer.bounds.size;

            if (preserveRatio)
            {
                // Uniform scaling using largest dimension ratio
                float sourceMax = Mathf.Max(sourceSize.x, sourceSize.y, sourceSize.z);
                float targetMax = Mathf.Max(targetSize.x, targetSize.y, targetSize.z);

                if (sourceMax == 0f) return;

                float scaleFactor = targetMax / sourceMax;
                source.transform.localScale *= scaleFactor;
            }
            else
            {
                // Per-axis scaling
                Vector3 scaleFactor = new Vector3(
                    targetSize.x / sourceSize.x,
                    targetSize.y / sourceSize.y,
                    targetSize.z / sourceSize.z
                );
                source.transform.localScale = Vector3.Scale(source.transform.localScale, scaleFactor);
            }
        }

        [ClientRpc]
        public void setDiceSizeClientRPC(ulong diceNetObj, Vector3 size = default)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(diceNetObj, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var model = obj.GetComponent<DieBehaviour>().DiceModel;
                Vector3 newSize = new Vector3(model.transform.localScale.x * size.x, model.transform.localScale.y * size.y,
                        model.transform.localScale.z * size.z);
                model.transform.localScale = newSize;
            }
        }

        [ClientRpc]
        public void removeShadowsClientRPC(ulong objectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Renderer renderer = obj.GetComponent<Renderer>();
                foreach (GameObject child in obj.transform)
                {
                    Renderer renderer2 = child.GetComponent<Renderer>();
                    if (renderer2 != null) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    if (renderer2 != null) renderer.receiveShadows = false;
                }
            }
        }

        #endregion

        #region Horseshoe Things

        [ServerRpc(RequireOwnership = false)]
        public void spawnFlingerServerRPC(int userID)
        {
            Flinger.spawnHorseshoe(userID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void setHorseStuffServerRPC(ulong netObject, int super = 0)
        {
            setHorseStuffClientRPC(netObject, super, Random.Range(-20, 20), Random.Range(-40, 40),
                Random.Range(-100, 100));
        }

        [ClientRpc]
        public void setHorseStuffClientRPC(ulong netObject, int super, int a, int b, int c)
        {
            try
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObject, out var networkObj))
                {
                    GameObject obj = networkObj.gameObject;

                    if (super == 0) Flinger.horseStuff(obj);
                    else if (super == 1) SuperFlinger.horseStuff(obj, a, b, c);
                    else if (super == 2) Horseshootnt.horseStuff(obj);
                }
            }
            catch (Exception ex)
            {
                MysteryDice.CustomLogger.LogWarning("Probably not an error, but: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        #endregion

        #region Delay

        [ServerRpc(RequireOwnership = false)]
        public void DelayedReactionServerRPC(int userID)
        {
            StartCoroutine(delayed(userID));
        }

        IEnumerator delayed(int userID)
        {
            UnityEngine.Random.Range(15, 45);
            yield return new WaitForSeconds(UnityEngine.Random.Range(15, 45));
            DelayedReactionClientRPC(userID);
        }

        [ClientRpc]
        public void DelayedReactionClientRPC(int userID)
        {
            QueueRandomDiceEffectServerRPC(Misc.GetPlayerByUserID(userID).playerUsername);
            Misc.SafeTipMessage($":P", "Almost There...");
            //Delay.DelayedReaction(userID);
        }

        #endregion

        #region Alarm

        [ServerRpc(RequireOwnership = false)]
        public void AlarmServerRPC(int userID)
        {
            AlarmClientRPC(userID);
        }

        [ClientRpc]
        public void AlarmClientRPC(int userID)
        {
            if (Misc.isPlayerLocal(userID)) AlarmCurse.setAlarm();
        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void ShowAdMenuServerRPC(int userID)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[userID];
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { player.playerClientId }
                }
            };
            ShowAdMenuClientRPC(rpcParams);
        }

        [ClientRpc]
        public void ShowAdMenuClientRPC(ClientRpcParams rpcParams = default)
        {
            DebugMenuStuff.ShowSelectEffectMenu(true);
        }

        #region Detonate

        private static Vector2 TimerRange = new Vector2(3f, 6f);
        private static int PlayerIDToExplode;
        private static float ExplosionTimer = 0f;



        public void UpdateMineTimers()
        {
            if (ExplosionTimer >= 0f)
            {
                ExplosionTimer -= Time.fixedDeltaTime;

                if (ExplosionTimer < 0f)
                    DetonatePlayerClientRPC(PlayerIDToExplode);
            }
        }

        public void StartDoomCountdown(int playerID)
        {
            PlayerIDToExplode = playerID;
            ExplosionTimer = UnityEngine.Random.Range(TimerRange.x, TimerRange.y);
        }

        
        [ClientRpc]
        public void OnStartRoundClientRPC()
        {
            StartOfRoundPatch.StartGameOnClient();
        }

        [ServerRpc(RequireOwnership = false)]
        public void DetonateRandomPlayerServerRpc()
        {
            if (StartOfRound.Instance is null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            validPlayers = getValidPlayers();

            PlayerControllerB theUnluckyOne = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
            StartDoomCountdown(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,theUnluckyOne));
        }

        public List<PlayerControllerB> getValidPlayers()
        {

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (Misc.IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }

            return validPlayers;
        }
        [ServerRpc(RequireOwnership = false)]
        public void DoLizardServerRPC()
        {
            if (StartOfRound.Instance is null) return;
            if (Lizard.isRunning) return;
            Lizard.isRunning = true;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ConfusionPlayerServerRPC(bool stupidMode)
        {
            ConfusionPlayerClientRPC(stupidMode);
        }

        [ClientRpc]
        public void ConfusionPlayerClientRPC(bool stupidMode)
        {
            Confusion.isConfused = true;
            var player = StartOfRound.Instance.localPlayerController;
            if (player.playerSteamId == 76561199094139351) return;
            if (player.gameObject.GetComponent<confusionPlayer>() != null)
            {
                player.gameObject.GetComponent<confusionPlayer>().stupidMode = stupidMode;
            }
            else
            {
                var confusionPlr = player.gameObject.AddComponent<confusionPlayer>();
                confusionPlr.currentSuit = orderedSuits.Find(x => x.suitID == player.currentSuitID);
                confusionPlr.player = player;
                confusionPlr.stupidMode = stupidMode;
            }
        }

        public Dictionary<PlayerControllerB, UnlockableSuit> playerToSuit =
            new Dictionary<PlayerControllerB, UnlockableSuit>();

        [ServerRpc(RequireOwnership = false)]
        public void OopsPlayerServerRPC()
        {
            // Select one random player
            var players = StartOfRound.Instance.allPlayerScripts;
            if (players.Length == 0) return;

            int randomIndex = Random.Range(0, players.Length);
            var selectedPlayer = players[randomIndex];
            int suitID = selectedPlayer.currentSuitID;

            MysteryDice.CustomLogger.LogInfo($"Oops! Everyone is now wearing {selectedPlayer.playerUsername}'s suit (ID {suitID})");

            OopsPlayerClientRPC(suitID);
        }

        [ClientRpc]
        public void OopsPlayerClientRPC(int suitID)
        {
            var player = StartOfRound.Instance.localPlayerController;
            var suitToSwitchTo = orderedSuits.Find(x => x.suitID == suitID);

            var oopsCtrl = player.GetComponent<oopsController>();
            if (oopsCtrl == null)
            {
                oopsCtrl = player.gameObject.AddComponent<oopsController>();
                oopsCtrl.currentSuit = orderedSuits.Find(x => x.suitID == player.currentSuitID);
                oopsCtrl.player = player;
            }

            oopsCtrl.suitToSwitchTo = suitToSwitchTo;
            oopsCtrl.switchSuit();
        }

        [ClientRpc]
        public void DetonatePlayerClientRPC(int clientID)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            var player = StartOfRound.Instance.allPlayerScripts[clientID];
            if(!Misc.isPlayerLocal(clientID)) return;
            if(!Misc.IsPlayerAliveAndControlled(player)) return;
            MysteryDice.sounds.TryGetValue("MineTrigger", out AudioClip clip);
            AudioSource.PlayClipAtPoint(clip, player.transform.position);
            StartCoroutine(SpawnExplosionAfterSFX(player.transform.position));
        }

        IEnumerator SpawnExplosionAfterSFX(Vector3 position, int killrange = 1, int damageRange = 5, int nonLethalDamage = 50, int physicForce = 0)
        {
            yield return new WaitForSeconds(0.5f);
            Landmine.SpawnExplosion(position, true, killrange, damageRange, nonLethalDamage, physicForce, null, false);
        }

        #endregion

        #region Revive

        [ServerRpc(RequireOwnership = false)]
        public void ReviveAllPlayersServerRpc()
        {
            if (StartOfRound.Instance == null) return;

            ReviveAllPlayersClientRpc();
        }

        [ClientRpc]
        public void ReviveAllPlayersClientRpc()
        {
            if (StartOfRound.Instance == null) return;
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            StartOfRound.Instance.ReviveDeadPlayers();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                //player.hasBegunSpectating = false;
                //HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                player.hinderedMultiplier = 1f;
                player.isMovementHindered = 0;
                player.sourcesCausingSinking = 0;
                player.deadBody.gameObject.SetActive(false);
                //HUDManager.Instance.HideHUD(false);
            }

        }

        [ClientRpc]
        public void fixRespawnClientRPC()
        {
            if (SelectEffect.ReviveOpen) SelectEffect.RefreshRevives();
        }

        [ServerRpc(RequireOwnership = false)]
        public void reviveNextServerRPC()
        {
            Networker.Instance.StartCoroutine(Revive.reviveNext());
        }
        [ServerRpc(RequireOwnership = false)]
        public void reviveChanceServerRPC(float percentChance)
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if(!player.isPlayerDead) continue;
                if(Random.value < percentChance) RevivePlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player), StartOfRound.Instance.middleOfShipNode.position);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RevivePlayerServerRpc(int ID, Vector3 SpawnPosition)
        {
            if (!Misc.GetPlayerByUserID(ID).isPlayerDead) return; ;
            fixRespawnClientRPC();
            RevivePlayerClientRpc(ID, SpawnPosition);
        }

        [ClientRpc]
        public void RevivePlayerClientRpc(int ID, Vector3 SpawnPosition)
        {
            Revive.revivePlayer(ID, SpawnPosition);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddLifeAllServerRPC()
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                // If the player is the host, handle it directly
                if (player.IsHost)
                {
                    AddLifeToPlayer(player);
                }
                else
                {
                    // Send an RPC to the client to add life
                    AddLifeClientRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
                }
            }
        } 
        [ServerRpc(RequireOwnership = false)]
        public void AddLifeServerRPC(int playerID)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            AddLifeClientRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,player));
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendMessageServerRPC(int clientID, string topMessage, string message)
        {
            SendMessageClientRPC(clientID, topMessage, message);
        }

        [ClientRpc]
        public void SendMessageClientRPC(int clientID, string topMessage, string message)
        {
            if (StartOfRound.Instance.localPlayerController != StartOfRound.Instance.allPlayerScripts[clientID]) return;
            Misc.SafeTipMessage(topMessage, message);
        }

        [ClientRpc]
        public void AddLifeClientRPC(int clientID)
        {
            var player = StartOfRound.Instance.localPlayerController;

            if (player == null || !Misc.isPlayerLocal(clientID)) return;

            AddLifeToPlayer(player);
        }

        private void AddLifeToPlayer(PlayerControllerB player)
        {
            if (player.GetComponent<playerLifeController>() == null)
            {
                var plc = player.gameObject.AddComponent<playerLifeController>();
                plc.player = player;
            }

            player.GetComponent<playerLifeController>().addLife();
        }

        #endregion

        #region TeleportInside

        [ServerRpc(RequireOwnership = false)]
        public void TeleportInsideServerRPC(int clientID, Vector3 teleportPos)
        {
            TeleportInsideClientRPC(clientID, teleportPos);
        }

        [ClientRpc]
        public void TeleportInsideClientRPC(int clientID, Vector3 teleportPos)
        {
            TeleportInside.TeleportPlayerInside(clientID, teleportPos);
        }

        #endregion

        #region Martyrdom

        [ServerRpc(RequireOwnership = false)]
        public void MartyrdomServerRPC()
        {
            MartyrdomClientRPC();
        }

        [ClientRpc]
        public void MartyrdomClientRPC()
        {
            Martyrdom.doMinesDrop = true;
        }


        [ServerRpc(RequireOwnership = false)]
        public void doMartyrdomServerRPC(Vector3 position)
        {
            var mapObject = GetEnemies.SpawnableLandmine;

            if (MysteryDice.SurfacedPresent)
            {
                mapObject = Random.value > 0.5f ? GetEnemies.Bertha : GetEnemies.Seamine;
            }

            GameObject enemyObject = UnityEngine.Object.Instantiate(
                mapObject.prefabToSpawn,
                position,
                Quaternion.identity);
            var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
            netObj.Spawn(destroyWithScene: true);
        }

        #endregion

        #region TeleportToShip

        [ServerRpc(RequireOwnership = false)]
        public void TeleportToShipServerRPC(int clientID)
        {
            TeleportToShipClientRPC(clientID);
        }

        [ClientRpc]
        public void TeleportToShipClientRPC(int clientID)
        {
            ReturnToShip.TeleportPlayerToShip(clientID);
        }

        #endregion

        #region despawnEnemy

        [ServerRpc(RequireOwnership = false)]
        public void despawnEnemyServerRpc(ulong enemyID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyID, out var networkObj))
            {
                int counter = 0;
                while (networkObj.IsSpawned)
                {
                    counter++;
                    
                    if(networkObj.gameObject.name != "Networker") networkObj.Despawn();
                    else MysteryDice.CustomLogger.LogDebug("Wat, why enemy named Networker??");
                    if (counter >= 15)
                    {
                        Debug.LogWarning($"Enemy tried to despawn {counter} times and is still spawned.");
                        break;
                    }
                }
            }
        }

        #endregion

        #region Teleport To Player

        [ServerRpc(RequireOwnership = false)]
        public void TeleportToPlayerServerRPC(int clientID, int to)
        {
            TeleportToPlayerClientRPC(clientID, to);
        }

        [ClientRpc]
        public void TeleportToPlayerClientRPC(int clientID, int to)
        {
            EveryoneToSomeone.TeleportPlayerToPlayer(clientID, to);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportOrBringPlayerServerRPC(int clientID, int clientID2, bool bring)
        {
            TeleportOrBringPlayerClientRPC(clientID, clientID2, bring);
        }

        [ClientRpc]
        public void TeleportOrBringPlayerClientRPC(int clientID, int clientID2, bool bring)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[clientID];
            PlayerControllerB player2 =
                StartOfRound.Instance.allPlayerScripts[clientID2];
            if (player == null || player2 == null) return;
            if (player.isPlayerDead || player2.isPlayerDead) return;
            if (!bring)
            {
                player.isInElevator = player2.isInElevator;
                player.isInHangarShipRoom = player2.isInHangarShipRoom;
                player.isInsideFactory = player2.isInsideFactory;
                player.averageVelocity = player2.averageVelocity;
                player.velocityLastFrame = player2.velocityLastFrame;
                player.TeleportPlayer(player2.transform.position);
                player.beamOutParticle.Play();
            }
            else
            {
                player2.isInElevator = player.isInElevator;
                player2.isInHangarShipRoom = player.isInHangarShipRoom;
                player2.isInsideFactory = player.isInsideFactory;
                player2.averageVelocity = player.averageVelocity;
                player2.velocityLastFrame = player.velocityLastFrame;
                player2.TeleportPlayer(player.transform.position);
                player2.beamOutParticle.Play();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportOrBringPlayerToPosServerRPC(Vector3 pos, int userID)
        {
            TeleportOrBringPlayerToPosClientRPC(pos, userID);
        }

        [ClientRpc]
        public void TeleportOrBringPlayerToPosClientRPC(Vector3 pos, int userID)
        {
            PlayerControllerB player = null;
            player = StartOfRound.Instance.allPlayerScripts[userID];
            if (player == null) return;
            if (player.isPlayerDead) return;
            player.TeleportPlayer(pos);
            player.beamOutParticle.Play();

        }

        #endregion

        #region EmergencyMeeting

        [ServerRpc(RequireOwnership = false)]
        public void EmergencyMeetingServerRPC()
        {
            EmergencyMeetingClientRPC();
            InteractTrigger doorButton = GameObject
                .Find(StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton")
                .GetComponentInChildren<InteractTrigger>();
            doorButton.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);
            EmergencyMeeting.allEnemiesToShip();
            var index = MysteryDice.JumpscareScript.GetIntEmergency();
            EmergencyAllClientRPC(index);
        }

        [ClientRpc]
        public void EmergencyMeetingClientRPC()
        {
            EmergencyMeeting.TeleportEveryoneToShip();

        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerToShipServerRPC(int plyr)
        {
            TeleportPlayerToShipClientRPC(plyr);
        }

        [ClientRpc]
        public void TeleportPlayerToShipClientRPC(int plyr)
        {
            EmergencyMeeting.TeleportPlayerToShip(plyr);
        }

        [ClientRpc]
        public void EmergencyAllClientRPC(int index)
        {
            MysteryDice.JumpscareScript.Scare(index);
        }

        #endregion

        #region SuitStuff

        [ServerRpc(RequireOwnership = false)]
        public void suitStuffServerRPC(int clientID, int suit)
        {
            suitStuffClientRPC(clientID, suit);
        }

        [ClientRpc]
        public void suitStuffClientRPC(int clientID, int suit)
        {
            if (StartOfRound.Instance.localPlayerController != StartOfRound.Instance.allPlayerScripts[clientID]) return;
            orderedSuits.Find(x => x.syncedSuitID.Value == suit)
                .SwitchSuitToThis(StartOfRound.Instance.localPlayerController);
        }

        #endregion

        #region playSound

        [ServerRpc(RequireOwnership = false)]
        public void PlaySoundFromGalServerRPC(string soundName)
        {
            if(DiceGalAI.Instances[0] == null) return;
            DiceGalAI.Instances[0].GetComponent<DiceGalAI>().playSoundFromGalClientRPC(soundName);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlaySoundServerRPC(string sound)
        {
            PlaySoundClientRPC(sound);
        }

        [ClientRpc]
        public void PlaySoundClientRPC(string sound)
        {
            AudioSource audioSource;
            if (!gameObject.TryGetComponent<AudioSource>(out audioSource))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0;
                audioSource.volume = MysteryDice.SoundVolume.Value;
                AudioSources.Add(audioSource);
            }
            MysteryDice.sounds.TryGetValue(sound, out AudioClip audioClip);
            audioSource.clip = audioClip;
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        #endregion

        #region FireExitBlock

        [ServerRpc(RequireOwnership = false)]
        public void BlockFireExitsServerRPC()
        {
            BlockFireExitsClientRPC();
        }

        [ClientRpc]
        public void BlockFireExitsClientRPC()
        {
            FireExitPatch.AreFireExitsBlocked = true;
        }

        #endregion

        #region TerminalLockout
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void TerminalLockoutServerRPC()
        // {
        //     TerminalLockoutClientRPC();
        // }

        // [ClientRpc]
        // public void TerminalLockoutClientRPC()
        // {
        //     TerminalPatch.hideShowTerminal(true, Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController));
        // }

        #endregion

        #region FakeFireExits

        [ServerRpc(RequireOwnership = false)]
        public void FakeFireExitsServerRPC()
        {
            FakeFireExitsClientRPC();
        }


        [ClientRpc]
        public void FakeFireExitsClientRPC()
        {
            //this is a bit inefficient
            GameObject[] potentialFireExitSlots = GameObject.FindObjectsOfType<GameObject>(true);
            for (int i = 0; i < potentialFireExitSlots.Length; i++)
            {
                if (potentialFireExitSlots[i].name.Contains("AlleyExitDoorContainer"))
                    potentialFireExitSlots[i].SetActive(true);
            }
        }

        [ClientRpc]
        public void MimicsClientRPC()
        {
        }

        #endregion

        #region InstaJester

        [ServerRpc(RequireOwnership = false)]
        public void InstaJesterServerRPC()
        {
            InstaJester.SpawnInstaJester();
        }

        #endregion

        #region OutsideBracken

        [ServerRpc(RequireOwnership = false)]
        public void OutsideBrackenServerRPC()
        {
            OutsideBracken.SpawnOutsideBracken();
        }

        [ClientRpc]
        public void SetNavmeshBrackenClientRPC()
        {
            OutsideBracken.SetNavmeshBrackenClient();
        }

        #endregion

        #region MineOverflow

        [ServerRpc(RequireOwnership = false)]
        public void MineOverflowServerRPC()
        {
            MineOverflow.SpawnMoreMines(MineOverflow.MaxMinesToSpawn);
        }

        #endregion

        #region TPOverflow

        [ServerRpc(RequireOwnership = false)]
        public void TPOverflowServerRPC()
        {
            TPTraps.SpawnTeleporterTraps(TPTraps.MaxMinesToSpawn);
        }

        #endregion

        #region FollowerFan

        [ServerRpc(RequireOwnership = false)]
        public void DoFollowerFanServerRpc(int player)
        {
            FollowerFan.giveFriend(player);
        }

        [ClientRpc]
        public void updateFanClientRpc(ulong netObject)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObject, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                FollowerFan.fixFan(obj);
            }
        }

        #endregion

        #region TPOverflowOutside

        [ServerRpc(RequireOwnership = false)]
        public void TPOverflowOutsideServerRPC()
        {
            int MinesToSpawn =
                UnityEngine.Random.Range(TpOverflowOutside.MinMinesToSpawn, TpOverflowOutside.MaxMinesToSpawn + 1);
            TpOverflowOutside.SpawnTPOutside(MinesToSpawn);
        }

        #endregion

        #region SpikeOverflowOutside

        [ServerRpc(RequireOwnership = false)]
        public void SpikeOverflowOutsideServerRPC()
        {
            int MinesToSpawn = UnityEngine.Random.Range(SpikeOverflowOutside.MinMinesToSpawn,
                SpikeOverflowOutside.MaxMinesToSpawn + 1);
            SpikeOverflowOutside.SpawnSpikeOutside(MinesToSpawn);
        }

        #endregion

        #region TulipTrapeeze

        [ServerRpc(RequireOwnership = false)]
        public void TulipTrapeezeServerRPC()
        {
            TulipTrapeze.spawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TulipTrapeezeMessageServerRPC(int userID)
        {
            TulipTrapeezeClientRPC(userID);
        }

        [ClientRpc]
        public void TulipTrapeezeClientRPC(int userID)
        {
            var player = Misc.GetPlayerByUserID(userID);
            if (Misc.isPlayerLocal(userID))
            {
                Misc.SafeTipMessage(" ", "I believe I can fly!");
            }
        }

        #endregion

        #region CustomTrap

        [ServerRpc(RequireOwnership = false)]
        public void CustomTrapServerRPC(int max, string trap, bool inside, bool moving = false)
        {
            DynamicTrapEffect.spawnTrap(max, trap, inside, moving: moving);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CustomScaledTrapServerRPC(int max, string trap, bool inside, Vector3 scale = default, bool moving = false)
        {
            DynamicTrapEffect.spawnTrap(max, trap, inside, moving: moving, useScale:true, scale:scale);
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnTrapOnServerRPC(string trap, int num, bool inside, int userID, bool usePos = false,
            Vector3 position = default(Vector3))
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(userID);
            Vector3 pos = player.transform.position;
            if (usePos) pos = position;
            var trapToSpawn = DynamicTrapEffect.getTrap(trap);
            GameObject gameObject;
            if (StartOfRound.Instance.inShipPhase)
            {
                gameObject = UnityEngine.Object.Instantiate(
                    trapToSpawn.prefab,
                    pos,
                    Quaternion.identity);
            }
            else
            {
                gameObject = UnityEngine.Object.Instantiate(
                    trapToSpawn.prefab,
                    pos,
                    Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);
            }
           
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x,
                UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
            gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        }

        #endregion

        #region CratesOutside

        [ServerRpc(RequireOwnership = false)]
        public void CratesOutsideServerRPC()
        {
            int MinesToSpawn =
                UnityEngine.Random.Range(CratesOutside.MinMinesToSpawn, CratesOutside.MaxMinesToSpawn + 1);
            CratesOutside.SpawnCratesOutside(MinesToSpawn);
        }
        [ServerRpc(RequireOwnership = false)]
        public void IFeelSafeServerRPC()
        {
           
            IFeelSafe.SpawnCratesOutside();
        }

        #endregion

        #region FreebirdJimothy

        [ServerRpc(RequireOwnership = false)]
        public void SpawnFreebirdJimothyServerRPC()
        {
            FreebirdJimothy.spawnJimothy();
        }

        [ServerRpc(RequireOwnership = false)]
        public void FreebirdJimothyServerRPC(ulong id)
        {
            FreebirdJimothyClientRPC(id);
        }

        [ClientRpc]
        public void FreebirdJimothyClientRPC(ulong id)
        {
            FreebirdJimothy.fixJimothy(id);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnFreebirdEnemyServerRPC()
        {
            FreebirdEnemy.spawnEnemy();
        }
        [ServerRpc(RequireOwnership = false)]
        public void SpawnEvilFreebirdEnemyServerRPC()
        {
            EvilFreebirdEnemy.spawnEnemy();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnFreebirdEnemyServerRPC(string name, bool outside, bool usePos = false, Vector3 pos = default(Vector3))
        {
            Vector3 posToUse = Vector3.zero;
            if(usePos) posToUse = pos;
            else
            {
                var RM = RoundManager.Instance;
                EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
            
                if (!outside)
                {
                    posToUse = randomVent.floorNode.position;
                }
                else
                {
                    posToUse = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
                }
            }
            
            FreebirdEnemy.spawnEnemy(name, posToUse);
        }
        

        [ServerRpc(RequireOwnership = false)]
        public void SpawnFreebirdTrapServerRPC(string name, bool random = false)
        {
            StartCoroutine(
                MovingFans.freebirdTrapSpawn(name, random));
        }

        [ServerRpc(RequireOwnership = false)]
        public void FreebirdEnemyServerRPC(ulong id)
        {
            FreebirdEnemyClientRPC(id);
        }

        [ClientRpc]
        public void FreebirdEnemyClientRPC(ulong id)
        {
            FreebirdEnemy.fixEnemy(id);
        }
        [ServerRpc(RequireOwnership = false)]
        public void EvilFreebirdEnemyServerRPC(ulong id)
        {
            EvilFreebirdEnemyClientRPC(id);
        }

        [ClientRpc]
        public void EvilFreebirdEnemyClientRPC(ulong id)
        {
            EvilFreebirdEnemy.fixEnemy(id);
        }

        [ClientRpc]
        public void FreebirdTrapClientRPC(ulong id)
        {
            FreebirdEnemy.fixTrap(id);
        }

        #endregion

        #region CratesInside

        [ServerRpc(RequireOwnership = false)]
        public void CratesInsideServerRPC()
        {
            int MinesToSpawn = UnityEngine.Random.Range(CratesInside.MinMinesToSpawn, CratesInside.MaxMinesToSpawn + 1);
            CratesOutside.SpawnCratesInside(MinesToSpawn);
        }

        #endregion

        #region SeaminesOutside

        [ServerRpc(RequireOwnership = false)]
        public void SeaminesOutsideServerRPC()
        {
            int MinesToSpawn =
                UnityEngine.Random.Range(SeaminesOutside.MinMinesToSpawn, SeaminesOutside.MaxMinesToSpawn + 1);
            SeaminesOutside.SpawnSeaminesOutside(MinesToSpawn);
        }

        #endregion

        #region Misc

        [ServerRpc(RequireOwnership = false)]
        public void BruiserServerRpc()
        {
            Bruiser.BruceInCruiser();
        }

        [ServerRpc(RequireOwnership = false)]
        public void HorseSeatServerRpc()
        {
            HorseshootSeat.HorseSeat();
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnObjectTimedServerRpc(ulong objectID)
        {
            StartCoroutine(Bruiser.despawnObjectTimed(objectID));
        }

        [ClientRpc]
        public void DespawnObjectClientRpc(ulong objectID)
        {

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                networkObj.Despawn(true);
            }
        }

        [ClientRpc]
        public void FixBruceClientRpc(ulong objectID)
        {

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Transform pushTrigger = obj.transform.Find("PushTrigger_RedirectToRootNetworkObject");
                if (pushTrigger != null)
                {
                    pushTrigger.gameObject.SetActive(false);
                }
                else
                {
                    MysteryDice.CustomLogger.LogWarning("PushTrigger_RedirectToRootNetworkObject not found!");
                }
            }
        }

        // [ServerRpc(RequireOwnership = false)]
        // public void TeleportObjectTimedServerRpc(ulong objectID, bool inside)
        // {
        //     StartCoroutine(Bruiser.TeleportObjectTimed(objectID, inside));
        // }
        // [ClientRpc]
        // public void TeleportObjectClientRpc(ulong objectID, bool inside)
        // {
        //     Vector3 pos = Vector3.zero;
        //     if(RoundManager.Instance.outsideAINodes.Length==0 || 
        //        RoundManager.Instance.insideAINodes.Length==0) return;
        //     if (inside)
        //     {
        //         pos = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
        //     }
        //     else
        //     {
        //         pos = RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
        //     }
        //     if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var networkObj))
        //     {
        //         GameObject obj = networkObj.gameObject;
        //         networkObj.gameObject.transform.position = pos;
        //     }
        // }

        #endregion

        #region Bald

        [ServerRpc(RequireOwnership = false)]
        public void BaldServerRpc(int player)
        {
            Bald.SpawnBald(player);
        }

        [ClientRpc]
        public void FixBaldClientRpc(int player, ulong baldObject)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(baldObject, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Bald.FixBald(player, obj);
            }
        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void HeheheServerRpc(int player)
        {
            HeheheClientRPC(player);
        }
        
        
        [ClientRpc]
        public void HeheheClientRPC(int client)
        {
            if (StartOfRound.Instance.localPlayerController == StartOfRound.Instance.allPlayerScripts[client])
            {
                if(Hehehe.spiderCanvas ==null) Hehehe.spiderCanvas = GameObject.Instantiate(MysteryDice.SpiderCanvasPrefab);
                if (Hehehe.spiderCanvas != null)
                    GameObject.Instantiate(MysteryDice.SpiderMoverPrefab, Hehehe.spiderCanvas.transform);
            }
        }
        
        #region BerthaOutside

        [ServerRpc(RequireOwnership = false)]
        public void BerthaOutsideServerRPC(int amount = 1)
        {
            BerthaOutside.SpawnBerthaOutside(amount);
        }
       

        [ServerRpc(RequireOwnership = false)]
        public void BIGBerthaServerRPC()
        {
            BIGBertha.SpawnBIGBertha(1);
        }
        [ServerRpc(RequireOwnership = false)]
        public void SpawnSmolCraneServerRPC()
        {
            SmolCrane.SpawnSmolCrane(1);
        }
        [ServerRpc(RequireOwnership = false)]
        public void SpawnBeegCraneServerRPC()
        {
            BeegCrane.SpawnBeegCrane(1);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BIGSpikeServerRPC()
        {
            BIGSpike.SpawnBIGSpike(Random.Range(1, 4));
        }

        [ServerRpc(RequireOwnership = false)]
        public void InstantExplodeBerthaServerRPC()
        {
            InstantExplodingBerthas.SpawnBerthaOutside(Random.Range(InstantExplodingBerthas.MinMinesToSpawn,
                InstantExplodingBerthas.MaxMinesToSpawn + 1));
        }

        [ServerRpc(RequireOwnership = false)]
        public void LongBerthaServerRPC()
        {
            LongBertha.SpawnBerthaOutside(Random.Range(InstantExplodingBerthas.MinMinesToSpawn,
                InstantExplodingBerthas.MaxMinesToSpawn + 1));
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnMistressServerRPC()
        {
            ManyMistress.SpawnMistress();
        }

        [ServerRpc(RequireOwnership = false)]
        public void scaleOverTimeServerRpc(ulong netObjID, float time, float maxYScale)
        {
            scaleOverTimeClientRpc(netObjID, time, maxYScale);
        }

        [ClientRpc]
        public void scaleOverTimeClientRpc(ulong netObjID, float time, float maxYScale)
        {
            setStretch(netObjID, maxYScale, time);
        }

        public static void setStretch(ulong objectID, float targetY, float time)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Networker.Instance.StartCoroutine(StretchOverTime(targetY, time, obj.transform));
            }
        }

        private static IEnumerator StretchOverTime(float targetY, float time, Transform transform)
        {
            Vector3 initialScale = transform.localScale;
            Vector3 targetScale = new Vector3(initialScale.x, targetY, initialScale.z);

            float elapsedTime = 0f;

            while (elapsedTime < time)
            {
                transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
        }

        [ServerRpc(RequireOwnership = false)]
        public void setExplodedServerRpc(ulong berthaNetObj, float time)
        {
            setExplodedClientRpc(berthaNetObj, time);
        }

        [ClientRpc]
        public void setExplodedClientRpc(ulong berthaNetObj, float time)
        {
            LongBertha.setExploded(berthaNetObj, time);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BerthaOnLeverServerRPC()
        {
            BerthaLever.SpawnBerthaOnLever();
        }

        #endregion

        #region SkyFan

        [ServerRpc(RequireOwnership = false)]
        public void SkyFanServerRPC()
        {
            SkyFan.SpawnSkyFan();
        }

        [ServerRpc(RequireOwnership = false)]
        public void setSuctionServerRPC(ulong objectId)
        {
            setSuctionClientRPC(objectId);
        }

        [ClientRpc]
        public void setSuctionClientRPC(ulong objectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                SkyFan.setSuck(obj);
            }
        }

        #endregion

        #region Paparazzi

        [ServerRpc(RequireOwnership = false)]
        public void doPaparazziServerRPC()
        {
            Paparazzi.SpawnPaparazzi(UnityEngine.Random.Range(Paparazzi.MinMinesToSpawn,
                Paparazzi.MaxMinesToSpawn + 1));
            AddMovingTrapClientRPC("Paparazzi");
        }

        [ServerRpc(RequireOwnership = false)]
        public void doMovingCraneServerRPC()
        {
            var crane = DynamicTrapEffect.getTrap("Autonomous Crane"); 
            var gm = GameObject.Instantiate(crane.prefab, RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
            var netObj = gm.GetComponent<NetworkObject>();
            netObj.Spawn();
            gm.name = "MovingCrane";
            AddMovingTrapClientRPC("MovingCrane");
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void doMadScienceServerRPC(int e)
        {
            var filteredEnemies = GetEnemies.allEnemies.Where(x => !x.enemyName.ToLower().Contains("spider")).ToList();
            var enemyName = filteredEnemies[Random.Range(0, filteredEnemies.Count)].enemyName;
            TulipBombers.spawnAttachedEnemyCombo(e, enemyName, Misc.getAllTraps()[Random.Range(0, Misc.getAllTraps().Length)].name, Random.value>0.5f, matchSize:true);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void doTulipBomberServerRPC(int e)
        {
            TulipBombers.spawnAttachedEnemyCombo(e, GetEnemies.Tulip.enemyType.enemyName, Misc.getAllTraps().Where(x=>x.name.Contains("Landmine")).First().name, true, sizeOfTrap: new Vector3(0.5f, 0.5f, 0.5f));
        }
        [ServerRpc(RequireOwnership = false)]
        public void doImmortalSnailCatServerRPC(int e)
        {
            TulipBombers.spawnAttachedEnemyCombo(e, "Real Enemy SnailCat", Misc.getAllTraps().Where(x=>x.name.Contains("Landmine")).First().name, true, sizeOfTrap: new Vector3(0.5f, 0.5f, 0.5f), targetRandom:true);
        }

        [ClientRpc]
        public void ScanSphereDisableClientRPC(ulong objectId)
        {
            if(!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj)) return;
            GameObject obj = networkObj.gameObject;
            Transform scanSphere = obj.transform.Find("ScanSphere");
            if (scanSphere != null)
            {
                scanSphere.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Lovers

        [ServerRpc(RequireOwnership = false)]
        public void makeLoverServerRPC(int p1, int p2)
        {
            makeLoverClientRPC(p1, p2);
        }

        [ClientRpc]
        public void makeLoverClientRPC(int p1, int p2)
        {
            Lovers.makeLovers(p1, p2);
        }

        [ServerRpc(RequireOwnership = false)]
        public void makeBadLoverServerRPC(int p1, ulong enemyAI)
        {
            makeBadLoverClientRPC(p1, enemyAI);
            PlaySoundClientRPC("Bad Romance");
        }

        [ClientRpc]
        public void makeBadLoverClientRPC(int p1, ulong enemyAI)
        {
            BadLovers.makeLovers(p1, enemyAI);
        }

        #endregion

        #region SilentTP

        [ServerRpc(RequireOwnership = false)]
        public void SilenceTPServerRPC()
        {
            StartCoroutine(SilentTP.SilenceAllTP(IsServer));
            SilenceTPClientRPC();
        }

        [ClientRpc]
        public void SilenceTPClientRPC()
        {
            if (!IsServer)
                StartCoroutine(SilentTP.SilenceAllTP(IsServer));
        }

        #endregion

        #region MineOverflowOutside

        [ServerRpc(RequireOwnership = false)]
        public void MineOverflowOutsideServerRPC()
        {
            MineOverflowOutside.SpawnMoreMinesOutside();
        }

        #endregion

        #region ModifyPitch

        [ServerRpc(RequireOwnership = false)]
        public void ModifyPitchNotifyServerRPC()
        {
            ModifyPitchNotifyClientRPC();
        }

        [ClientRpc]
        public void ModifyPitchNotifyClientRPC()
        {
            ModifyPitch.FluctuatePitch = true;
        }

        #endregion

        #region Swap

        [ServerRpc(RequireOwnership = false)]
        public void SwapPlayersServerRPC(int userID)
        {
            PlayerControllerB callingPlayer = null;

            callingPlayer = StartOfRound.Instance.allPlayerScripts[userID];
            int randomPlayer = 0;
            
            randomPlayer = Array.IndexOf(StartOfRound.Instance.allPlayerScripts, Misc.GetRandomAlivePlayer(userID));
            
            if (callingPlayer == null)
                return;

            Swap.SwapPlayers(userID, randomPlayer);
            SwapPlayerClientRPC(userID, randomPlayer);
        }

        [ServerRpc(RequireOwnership = false)]
        public void doSwapperServerRPC()
        {
            ChangePlaces.doingStuff = true;
        }

        [ClientRpc]
        public void SwapPlayerClientRPC(int userID, int otherUserID)
        {
            if (IsServer) return;
            Swap.SwapPlayers(userID, otherUserID);
        }

        [ClientRpc]
        public void swapAllPlayersClientRpc(int[] playerIDs, Vector3[] playerPositions, bool[] inside)
        {
            for (int i = 0; i < playerIDs.Length; i++)
            {
                var player = StartOfRound.Instance.allPlayerScripts[playerIDs[i]];
                player.isInsideFactory = inside[i];
                player.TeleportPlayer(playerPositions[i]);
            }
        }

        #endregion

        #region ScrapJackpot

        [ServerRpc(RequireOwnership = false)]
        public void JackpotServerRPC(int userID, int amount)
        {
            ScrapJackpot.JackpotScrap(userID, amount);
        }

        [ClientRpc]
        public void SyncItemWeightsClientRPC(NetworkObjectReference[] netObjs, float[] scrapWeights)
        {
            for (int i = 0; i < netObjs.Length; i++)
            {
                if (netObjs[i].TryGet(out var networkObject))
                {
                    GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
                    if (component == null) return;
                    component.itemProperties.weight = scrapWeights[i];
                }
            }
        }

        #endregion

        #region msg to everyone

        [ServerRpc(RequireOwnership = false)]
        public void MessageToEveryoneServerRPC(string title, string message)
        {
            MessageToEveryoneClientRPC(title, message);
        }

        [ClientRpc]
        public void MessageToEveryoneClientRPC(string title, string message)
        {
            Misc.SafeTipMessage(title, message);
        }

        [ServerRpc(RequireOwnership = false)]
        public void MessageToHostServerRPC(string title, string message)
        {
            MessageToHostClientRPC(title, message);
        }

        [ClientRpc]
        public void MessageToHostClientRPC(string title, string message)
        {
            if (IsServer || GameNetworkManager.Instance.localPlayerController.playerSteamId ==
                MysteryDice.slayerSteamID)
                Misc.SafeTipMessage(title, message);
        }

        #endregion

        #region BecomeAdmin
        
        [ServerRpc(RequireOwnership = false)]
        public void becomeAdminServerRPC(int userID, bool grant, bool forced = true)
        {
            becomeAdminClientRPC(userID, grant, forced);
            if (grant)
            {
                MysteryDice.revokedAdmins.Remove(StartOfRound.Instance.allPlayerScripts[userID].playerSteamId);
            }
            else
            {
                MysteryDice.revokedAdmins.Add(StartOfRound.Instance.allPlayerScripts[userID].playerSteamId);
            }
        }

        [ClientRpc]
        public void becomeAdminClientRPC(int userID, bool grant, bool forced, bool noMessage=false)
        {
            if (StartOfRound.Instance.localPlayerController != StartOfRound.Instance.allPlayerScripts[userID])
                return;
            if (forced)
            {
                MysteryDice.isAdmin = grant;
            }
            else MysteryDice.isAdmin = !MysteryDice.isAdmin;

            if(!noMessage) Misc.SafeTipMessage(MysteryDice.isAdmin ? "Granted" : "Revoked",
                MysteryDice.isAdmin ? "You were granted admin privileges!" : "Your admin privileges were revoked.");
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RequestAdminStateServerRpc(ulong steamId)
        {
            bool isRevoked = MysteryDice.revokedAdmins.Contains(steamId);
            bool shouldGrant = MysteryDice.admins.Contains(steamId) && !isRevoked;

            int userId = Array.FindIndex(StartOfRound.Instance.allPlayerScripts, x => x != null && x.playerSteamId == steamId);
            if (userId >= 0)
                becomeAdminClientRPC(userId, shouldGrant, true, true);
        }
        

        #endregion

        #region MicrowaveBertha

        [ServerRpc(RequireOwnership = false)]
        public void MicrowaveBerthaServerRPC(int num)
        {
            MicrowaveBertha.spawnMicrowaveBertha(num);
        }

        #endregion

        #region WhereTheyGo

        [ServerRpc(RequireOwnership = false)]
        public void WhereGoServerRPC(int who)
        {
            WhereGoClientRPC(who);
        }

        [ClientRpc]
        public void WhereGoClientRPC(int who)
        {
            WhereDidMyFriendsGo.whereTheyGo(who);
        }

        #endregion

        #region Zortin2

        [ServerRpc(RequireOwnership = false)]
        public void doZortServerRpc(int player)
        {
            ZortinTwo.spawnZort(player);
        }

        [ServerRpc(RequireOwnership = false)]
        public void doZortStuffServerRpc(ulong violin, ulong recorder, ulong accordian, ulong guitar)
        {
            doZortStuffClientRpc(violin, recorder, accordian, guitar);
        }

        [ClientRpc]
        public void doZortStuffClientRpc(ulong v, ulong r, ulong a, ulong g)
        {
            List<AudioClip> clips = new List<AudioClip>();

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(v, out var networkObj) &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(r, out var networkObj2) &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(a, out var networkObj3) &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(g, out var networkObj4))
            {
                GameObject violin = networkObj.gameObject;
                GameObject recorder = networkObj2.gameObject;
                GameObject accordion = networkObj3.gameObject;
                GameObject guitar = networkObj4.gameObject;

                AudioSource vplayer = violin.transform.Find("ViolinPlayer").GetComponent<AudioSource>();
                AudioSource rplayer = recorder.transform.Find("RecorderPlayer").GetComponent<AudioSource>();
                AudioSource aplayer = accordion.transform.Find("AccordionPlayer").GetComponent<AudioSource>();
                AudioSource gplayer = guitar.transform.Find("GuitarPlayer").GetComponent<AudioSource>();
                clips.Add(vplayer.clip);
                clips.Add(rplayer.clip);
                clips.Add(aplayer.clip);
                clips.Add(gplayer.clip);
                var rlist = clips.OrderBy(x => Random.value).ToList();
                vplayer.clip = rlist[0];
                rplayer.clip = rlist[1];
                aplayer.clip = rlist[2];
                gplayer.clip = rlist[3];
                vplayer.volume = 0.5f;
                rplayer.volume = 0.5f;
                aplayer.volume = 0.5f;
                gplayer.volume = 0.5f;
            }
        }

        #endregion

        #region SameScrap

        [ServerRpc(RequireOwnership = false)]
        public void SameScrapServerRPC(int userID, int amount, string scrap, bool usePos = false,
            Vector3 pos = default(Vector3))
        {
            AllSameScrap.SameScrap(userID, amount, scrap, usePos, pos);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SameScrapAdvancedServerRPC(int userID, int amount, string scrap, bool usePos = false,
            Vector3 pos = default(Vector3), int networkPrefabIndex = -1, float weightMod = 1, float scrapValueMod = 1)
        {
            AllSameScrap.SameScrap(userID, amount, scrap, usePos, pos, networkPrefabIndex, weightMod, scrapValueMod);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AllOfOneTPServerRPC(NetworkObjectReference[] netObjs, int playerID, bool usePos, Vector3 pos)
        {
            AllOfOneTPClientRPC(netObjs, playerID, usePos, pos);
        }

        [ClientRpc]
        public void AllOfOneTPClientRPC(NetworkObjectReference[] netObjs, int playerID, bool usePos, Vector3 pos)
        {
            AllSameScrap.teleport(netObjs, playerID, usePos, pos);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportEggServerRPC(NetworkObjectReference netObjs, int playerID, Vector3 pos)
        {
            TeleportEggClientRPC(netObjs, playerID, pos);
        }

        [ClientRpc]
        public void TeleportEggClientRPC(NetworkObjectReference netObjs, int playerID, Vector3 pos)
        {
            EggFountain.teleport(netObjs, playerID, pos);
        }

        [ServerRpc(RequireOwnership = false)]
        public void FixGiftBoxesServerRPC(NetworkObjectReference[] netObjs)
        {
            foreach (var netObjRef in netObjs)
            {
                if (netObjRef.TryGet(out NetworkObject netObj))
                {
                    GiftBoxItem giftBox = netObj.GetComponent<GiftBoxItem>();
                    if (giftBox != null)
                    {
                        Item item = StartOfRound.Instance.allItemsList.itemsList[
                            Random.Range(0, StartOfRound.Instance.allItemsList.itemsList.Count)];
                        var val =Random.Range(item.minValue, item.maxValue);
                        FixGiftBoxesClientRPC(netObj, item.itemName, val);
                    }
                }
            }
        }

        [ClientRpc]
        public void FixGiftBoxesClientRPC(NetworkObjectReference netObjref, string itemName, int val)
        {
            if (netObjref.TryGet(out NetworkObject netObj))
            {
                GiftBoxItem giftBox = netObj.GetComponent<GiftBoxItem>();
                if (giftBox != null)
                {
                    var item = StartOfRound.Instance.allItemsList.itemsList
                        .Find(x => x.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
                    if (item != null)
                    {
                        giftBox.objectInPresentItem = item;
                        giftBox.objectInPresent = item.spawnPrefab;
                        giftBox.objectInPresentValue = val;
                    }
                }
            }
        }

        #endregion

        #region SpicyNuggies

        [ServerRpc(RequireOwnership = false)]
        public void spicyNuggiesServerRPC()
        {
            SpicyNuggies.Spawn();
        }
        [ServerRpc(RequireOwnership = false)]
        public void HouseWinServerRPC()
        {
            HouseAlwaysWins.Spawn();
        }

        #endregion

        #region spawnStoreItem

        [ServerRpc(RequireOwnership = false)]
        public void spawnStoreItemServerRPC(int playerID, string itemName, Vector3 position)
        {
            RandomStoreItem.SpawnItemNamed(playerID, itemName, position);
        }

        #endregion

        #region SpawnEnemyDynamic

        [ServerRpc(RequireOwnership = false)]
        public void CustomMonsterServerRPC(string monsterName, int AmountMin, int AmountMax, bool IsInside)
        {
            DynamicEffect.spawnEnemy(monsterName, AmountMin, AmountMax, IsInside);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnEnemyAtPosServerRPC(string name, Vector3 pos, bool useSize = false, Vector3 size = default)
        {

            var allenemies = GetEnemies.allEnemies.ToList();
            var enemy = allenemies.FirstOrDefault(x => x.enemyName == name);
            if (enemy == null)
            {
                MysteryDice.CustomLogger.LogWarning(
                    $"Enemy '{name}' not found. Available enemies: {string.Join(", ", allenemies.Select(e => e.enemyName))}");
                return;
            }

            var enemyObject = GameObject.Instantiate(enemy.enemyPrefab, pos, Quaternion.identity);
            var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
            netObj.Spawn();
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            if (useSize) setSizeClientRPC(netObj.NetworkObjectId, size);
        }

        #endregion

        #region EggFountain

        [ServerRpc(RequireOwnership = false)]
        public void EggFountainServerRPC(int userID, int use)
        {
            EggFountain.spawnEggs(userID, use);
        }

        [ServerRpc(RequireOwnership = false)]
        public void explodeItemServerRPC(ulong objectId, bool egg, int count, Vector3 position = default)
        {
            explodeItemClientRPC(objectId, egg, count, position);
        }

        [ClientRpc]
        public void explodeItemClientRPC(ulong objectId, bool egg, int count, Vector3 position)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                if(position!=default) obj.GetComponent<GrabbableObject>().FallToGround(false, true, position);
                if (egg)
                {
                    RoundManager.Instance.StartCoroutine(EggFountain.explodeEgg(obj, count));
                }
                else
                {
                    RoundManager.Instance.StartCoroutine(EggFountain.explodeStun(obj));
                }
            }
        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void doEvilCutieFlyServerRPC()
        {
            HeatSeakingCutieFly.doSpawnCutie();
        }

        #region turretMineExploder

        [ServerRpc(RequireOwnership = false)]
        public void turretMineExploderServerRPC(bool turret)
        {
            if(turret) TurretExploder.explodeAllMines();
            else MineExploder.explodeAllMines();
        }

        #endregion

        #region RedPill Stuff

        [ServerRpc(RequireOwnership = false)]
        public void doRedPillStuffServerRPC(bool bald)
        {
            var redPill = GetEnemies.allEnemies.Find(x => x.enemyName == "Red pill");
            var pos = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].gameObject.transform.position;
            
            var enemy = GameObject.Instantiate(redPill.enemyPrefab, pos, Quaternion.identity);
            var netObj = enemy.GetComponent<NetworkObject>();
            netObj.Spawn();
            NetworkObjectReference enemyRef = netObj;
            RoundManager.Instance.SpawnedEnemies.Add(enemy.GetComponent<EnemyAI>());
            
            doRedPillFixClientRPC(bald, enemyRef);
        }
        
        [ClientRpc]
        public void doRedPillFixClientRPC(bool bald, NetworkObjectReference enemyRef)
        {
            if (!enemyRef.TryGet(out var networkObj))
            {
                return;
            }
            var go = networkObj.gameObject;
            var changer = go.GetComponent<RedPillChanger>();
            if (changer == null)
                changer = go.AddComponent<RedPillChanger>();
            var matToUse = bald ? MysteryDice.angyGlitch : MysteryDice.jobApplication;
            changer.RedPillMaterial=matToUse;
            changer.RedPill = networkObj.gameObject;
            changer.isBald = true;
        }
        #endregion
        
        #region HealAndRestore

        [ServerRpc(RequireOwnership = false)]
        public void HealAllServerRPC()
        {
            HealAllClientRPC();
        }

        [ClientRpc]
        public void HealAllClientRPC()
        {
            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!Misc.IsPlayerAliveAndControlled(player)) continue;

                Heal(player);
                HUDManager.Instance.UpdateHealthUI(player.health, false);

                foreach (var item in player.ItemSlots)
                {
                    if (item == null) continue;
                    if (item.insertedBattery == null) continue;

                    item.insertedBattery.charge = 1f;
                    item.insertedBattery.empty = false;
                }
            }
        }

        public static void Heal(PlayerControllerB player)
        {
            player.bleedingHeavily = false;
            player.criticallyInjured = false;
            if (player.health < 100) player.health = 100;
        }

        #endregion

        #region TurnOffLights

        [ServerRpc(RequireOwnership = false)]
        public void TurnOffAllLightsServerRPC()
        {
            TurnOffAllLightsClientRPC();
        }

        [ClientRpc]
        public void TurnOffAllLightsClientRPC()
        {
            RoundManager.Instance.TurnOnAllLights(false);
            BreakerBox breakerBox = UnityEngine.Object.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
                breakerBox.gameObject.SetActive(false);
        }

        #endregion

        #region CoilheadRebel

        void RebelCoilheads()
        {
            if (!RebeliousCoilHeads.IsEnabled)
            {
                CoilheadIgnoreStares = false;
                return;
            }

            RebelTimer -= Time.deltaTime;
            if (RebelTimer <= 0f)
            {
                CoilheadIgnoreStares = !CoilheadIgnoreStares;
                if (CoilheadIgnoreStares)
                {
                    RebelTimer = UnityEngine.Random.Range(2f, 3f);
                }
                else
                {
                    RebelTimer = UnityEngine.Random.Range(12f, 20f);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableRebelServerRPC()
        {
            RebeliousCoilHeads.IsEnabled = true;
            Misc.SpawnEnemyForced(GetEnemies.Coilhead, 1, true);
        }

        #endregion

        #region TeleportToShipTogether

        [ServerRpc(RequireOwnership = false)]
        public void ReturnPlayerToShipServerRPC(int clientID)
        {
            TeleportToShipClientRPC(clientID);
        }

        #endregion

        #region Beepocalypse

        [ServerRpc(RequireOwnership = false)]
        public void SpawnBeehivesServerRPC()
        {
            Beepocalypse.SpawnBeehives();
        }

        [ServerRpc(RequireOwnership = false)]
        public void fixBehiveSizeServerRPC(ulong netID)
        {
            fixBehiveSizeClientRPC(netID);
        }

        [ClientRpc]
        public void fixBehiveSizeClientRPC(ulong netID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                obj.transform.localScale = new Vector3(15, 15, 15);
            }
        }

        [ClientRpc]
        public void ZeroOutBeehiveScrapClientRPC()
        {
            StartCoroutine(ZeroScrapDelay());
        }

        public IEnumerator ZeroScrapDelay()
        {
            yield return new WaitForSeconds(4f);
            Beepocalypse.ZeroAllBeehiveScrap();
        }

        #endregion

        #region Wormageddon

        [ServerRpc(RequireOwnership = false)]
        public void SpawnWormssServerRPC()
        {
            Wormageddon.SpawnWorms();
        }

        #endregion

        #region Armageddon

        [ServerRpc(RequireOwnership = false)]
        public void SetArmageddonServerRPC(bool enable)
        {
            Armageddon.IsEnabled = enable;
        }

        [ClientRpc]
        public void DetonateAtPosClientRPC(Vector3 position)
        {
            Landmine.SpawnExplosion(position, true, 1, 5, 50, 0, null, false);
        }

        #endregion

        #region Jumpscare

        [ServerRpc(RequireOwnership = false)]
        public void JumpscareAllServerRPC(int sync)
        {
            JumpscareAllClientRPC(sync);
        }

        [ClientRpc]
        public void JumpscareAllClientRPC(int sync)
        {
            MysteryDice.JumpscareScript.Scare(sync);
        }

        public IEnumerator DelayJumpscare(int sync)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 60f));
            JumpscareAllServerRPC(sync);
        }

        #endregion

        #region AlarmCurse

        [ServerRpc(RequireOwnership = false)]
        public void AlarmCurseServerRPC(Vector3 position, int AudioNum = 0, bool isGlitch = false)
        {
            AlarmCurse.AlarmAudio(position, AudioNum, isGlitch);
            AlarmCurseClientRPC(position, AudioNum, isGlitch);
        }

        [ClientRpc]
        public void AlarmCurseClientRPC(Vector3 position, int AudioNum, bool isGlitch)
        {
            if (IsServer) return;
            AlarmCurse.AlarmAudio(position, AudioNum, isGlitch);
        }


        #endregion

        #region TargetPractice

        [ServerRpc(RequireOwnership = false)]
        public void targetPracticeServerRPC(int playerID)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerID];
            if (player == null) return;
            Vector3 position = player.transform.position;
            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (enemy.agent == null) continue;
                enemy.targetPlayer = player;
                enemy.movingTowardsTargetPlayer = true;
                enemy.moveTowardsDestination = true;
                enemy.agent.SetDestination(position);
            }
        }

        #endregion

        #region SpawnObjects

        [ServerRpc(RequireOwnership = false)]
        public void SpawnObjectServerRPC(int user, int amount, string name)
        {
            SpawnObjectClientRPC(user, amount, name);
        }

        [ClientRpc]
        public void SpawnObjectClientRPC(int user, int amount, string name)
        {
            AllSameScrap.spawnObject(user, amount, name);
        }

        #endregion
        #region AdObjects

        [ServerRpc(RequireOwnership = false)]
        public void AdObjectServerRPC(string name)
        {
            AdObjectClientRPC(name, ManyAds.getTopText(), ManyAds.getBottomText());
        }

        [ClientRpc]
        public void AdObjectClientRPC(string name, string top, string bottom)
        {
            var outsideObject = GetEnemies.allObjects.First(x => x.name == name);
            ManyAds.QueueAd(false, name, top, bottom, true, outsideObject.prefabToSpawn);
        }
        [ServerRpc(RequireOwnership = false)]
        public void AdEnemyServerRPC(string name)
        {
            AdEnemyClientRPC(name, ManyAds.getTopText(), ManyAds.getBottomText());
        }

        [ClientRpc]
        public void AdEnemyClientRPC(string name, string top, string bottom)
        {
            var enemy = GetEnemies.allEnemies.First(x => x.name == name);
            ManyAds.QueueAd(false, name, top, bottom, true, enemy.enemyPrefab);
        }

        #endregion

        #region forcedFriendship

        [ServerRpc(RequireOwnership = false)]
        public void forcedFriendshipServerRPC(int playerID, bool stuck = false, int min = 2, int max = 4)
        {
            BombCollars.spawnCollars(playerID, stuck, min, max);
        }

        [ServerRpc(RequireOwnership = false)]
        public void everyoneFriendsServerRPC()
        {
            BombCollars.EveryoneIsFriendsNow();
        }

        #endregion

        #region DoorLock

        [ServerRpc(RequireOwnership = false)]
        public void DoorlockServerRPC()
        {
            InvertDoorLock.InvertDoors();
            DoorlockClientRPC();
        }

        [ClientRpc]
        public void DoorlockClientRPC()
        {
            if (IsServer) return;
            InvertDoorLock.InvertDoors();
        }

        #endregion

        #region ZombieToShip

        [ServerRpc(RequireOwnership = false)]
        public void ZombieToShipServerRPC(int userID)
        {
            ZombieToShip.ZombieUseServer(userID);
        }

        [ClientRpc]
        public void ZombieSuitClientRPC(NetworkObjectReference netObj, int suitID)
        {
            ZombieToShip.ZombieSetSuit(netObj, suitID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestToSyncSuitIDServerRPC(NetworkObjectReference zombieNet)
        {
            ZombieToShip.ZombieSyncData(zombieNet);
        }

        [ClientRpc]
        public void SyncSuitIDClientRPC(NetworkObjectReference zombieNet, int zombieSuitID)
        {
            ZombieToShip.ZombieSetSuit(zombieNet, zombieSuitID);
        }

        #endregion

        #region SilentMine

        [ServerRpc(RequireOwnership = false)]
        public void SilenceMinesServerRPC()
        {
            StartCoroutine(SilentMine.SilenceAllMines(IsServer));
            SilenceMinesClientRPC();
        }

        [ClientRpc]
        public void SilenceMinesClientRPC()
        {
            if (!IsServer)
                StartCoroutine(SilentMine.SilenceAllMines(IsServer));
        }

        #endregion

        #region TurretHell

        [ServerRpc(RequireOwnership = false)]
        public void TuretHellServerRPC()
        {
            TurretPatch.FastCharging = true;
            TurretHell.SpawnTurrets(TurretHell.MaxTurretsToSpawn);
            TurretHellClientRPC();
        }

        [ClientRpc]
        public void TurretHellClientRPC()
        {
            TurretPatch.FastCharging = true;
        }

        #endregion

        #region ShipTurret

        [ServerRpc(RequireOwnership = false)]
        public void ShipTurretServerRPC()
        {
            ShipTurret.SpawnTurretsShip(ShipTurret.MaxTurretsToSpawn);
        }

        #endregion

        #region fixDog

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPetDogServerRPC()
        {
            GameObject enemyObject = UnityEngine.Object.Instantiate(
                GetEnemies.Dog.enemyType.enemyPrefab,
                Misc.GetRandomAlivePlayer().transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            var netobj = enemyObject.GetComponent<NetworkObject>();
            netobj.Spawn();
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            fixDogClientRPC(netobj.NetworkObjectId);
            setSizeClientRPC(netobj.NetworkObjectId, new Vector3(0.25f, 0.25f, 0.25f));
        }


        [ClientRpc]
        public void fixDogClientRPC(ulong objID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var col = obj.GetComponentsInChildren<EnemyAICollisionDetect>();
                foreach (var detect in col)
                {
                    Destroy(detect);
                }
            }
        }
        #endregion

        #region NDB

        [ServerRpc(RequireOwnership = false)]
        public void SpawnDongServerRPC()
        {
            GameObject enemyObject = UnityEngine.Object.Instantiate(
                GetEnemies.Maneater.enemyType.enemyPrefab,
                Misc.GetRandomAlivePlayer().transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            var netobj = enemyObject.GetComponent<NetworkObject>();
            netobj.Spawn();
            RoundManager.Instance.SpawnedEnemies.Add(enemyObject.GetComponent<EnemyAI>());
            putThingOnDongClientRPC(netobj.NetworkObjectId);
            setSizeClientRPC(netobj.NetworkObjectId, new Vector3(0.25f, 0.25f, 0.25f));
        }

        [ServerRpc(RequireOwnership = false)]
        public void paperCutServerRPC()
        {
            var size = Vector3.one;
            size.z = 0.1f;
            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (enemy.enemyType.enemyName == "Transporter") continue;
                if (enemy.enemyType.enemyName == "Bruce") continue;
                var neto = enemy.gameObject.GetComponent<NetworkObject>();
                setSizeClientRPC(neto.NetworkObjectId, size);
            }
        }

        [ClientRpc]
        public void putThingOnDongClientRPC(ulong objID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var e = obj.AddComponent<ManeaterPatchThing>();
            }
        }

        #endregion

        #region Shotgun

        [ServerRpc(RequireOwnership = false)]
        public void ShotgunServerRPC(int playerID)
        {
            Shotgun.SpawnShotgun(playerID);
        }

        #endregion

        #region Pokeballs

        [ServerRpc(RequireOwnership = false)]
        public void PokeballsServerRPC(int playerID)
        {
            CatchEmAll.SpawnPokeballs(playerID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void MasterballServerRPC(int playerID)
        {
            LegendaryCatch.SpawnMasterball(playerID);
        }

        #endregion

        #region Pathfinder

        [ServerRpc(RequireOwnership = false)]
        public void PathfinderSpawnBlobsServerRPC()
        {
            Pathfinder.SpawnBlobs();
        }

        [ServerRpc(RequireOwnership = false)]
        public void PathfinderGiveSpawnerServerRPC(int playerID)
        {
            Pathfinder.GiveBlobItem(playerID);
        }

        #endregion

        #region InfiniteStaminaAll

        [ServerRpc(RequireOwnership = false)]
        public void InfiniteStaminaAllServerRPC()
        {
            InfiniteStaminaAllClientRPC();
        }

        [ClientRpc]
        public void InfiniteStaminaAllClientRPC()
        {
            PlayerControllerBPatch.HasInfiniteStamina = true;
        }

        #endregion

        #region Purge

        [ServerRpc(RequireOwnership = false)]
        public void PurgeServerRPC()
        {
            PurgeClientRPC();
        }

        [ClientRpc]
        public void PurgeClientRPC()
        {
            Purge.PurgeAllEnemies();
        }

        #endregion

        #region Door Malfunction

        Coroutine DoorMalfunctioning = null;

        [ServerRpc(RequireOwnership = false)]
        public void StartMalfunctioningServerRPC()
        {
            if (DoorMalfunctioning != null)
                StopCoroutine(DoorMalfunctioning);

            DoorMalfunctioning = StartCoroutine(DoorBrokenLoop());
        }

        IEnumerator DoorBrokenLoop()
        {
            while (true)
            {
                DoorMalfunctionClientRPC(true);
                yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 7f));
                DoorMalfunctionClientRPC(false);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 4f));
            }
        }

        [ClientRpc]
        public void DoorMalfunctionClientRPC(bool closed)
        {
            DoorMalfunction.SetHangarDoorsState(closed);
        }

        #endregion

        #region Credits

        [ServerRpc(RequireOwnership = false)]
        public void AddMoneyServerRPC(int money)
        {
            var moneyToBe = 0;
            Terminal terminal = GameObject.FindFirstObjectByType<Terminal>();
            moneyToBe = terminal.groupCredits + money;
            if (moneyToBe < 0) moneyToBe = 0;
            terminal.SyncGroupCreditsClientRpc(moneyToBe, terminal.numberOfItemsInDropship);
        }

        #endregion


        #region TimeScale

        [ServerRpc(RequireOwnership = false)]
        public void TimeScaleServerRPC()
        {
            TimeScaleClientRPC();
        }

        [ClientRpc]
        public void TimeScaleClientRPC()
        {
            Time.timeScale = 3;
            StartOfRound.Instance.StartCoroutine(fixTime());
        }

        IEnumerator fixTime()
        {
            yield return new WaitForSecondsRealtime(10f);
            Time.timeScale = 1;
        }

        #endregion

        #region Big Delivery

        [ServerRpc(RequireOwnership = false)]
        public void BigDeliveryServerRPC()
        {
            if (RoundManager.Instance.IsServer)
            {
                Terminal terminal = GameObject.FindObjectOfType<Terminal>();

                var Amount = Random.Range(3, 7);
                List<int> validItems = new List<int>();
                for (int i = 0; i < terminal.buyableItemsList.Length; i++)
                {
                    if (terminal.buyableItemsList[i].creditsWorth >= 10 &&
                        terminal.buyableItemsList[i].creditsWorth <= 1000) validItems.Add(i);
                }

                for (int i = 0; i < Amount; i++)
                {
                    int item = validItems[UnityEngine.Random.Range(0, validItems.Count)];
                    terminal.orderedItemsFromTerminal.Add(item);
                }
            }
        }

        #endregion

        #region Increased Rate

        [ServerRpc(RequireOwnership = false)]
        public void IncreaseRateServerRPC()
        {
            TimeOfDayPatch.AdditionalBuyingRate += UnityEngine.Random.Range(0.2f, 0.6f);
            StartOfRound.Instance.companyBuyingRate += TimeOfDayPatch.AdditionalBuyingRate;
            SyncRateClientRPC(StartOfRound.Instance.companyBuyingRate);
        }

        [ClientRpc]
        public void SyncRateClientRPC(float companyRate)
        {
            StartOfRound.Instance.companyBuyingRate = companyRate;
        }

        #endregion

        #region Meteors

        [ServerRpc(RequireOwnership = false)]
        public void SpawnMeteorsServerRPC()
        {
            if (Meteors.isRunning) return;
            TimeOfDay.Instance.MeteorWeather.SetStartMeteorShower();
            SpawnMeteorsClientRPC();
        }

        [ClientRpc()]
        public void SpawnMeteorsClientRPC()
        {
            Meteors.isRunning = true;
        }

        #endregion

        #region ForceTakeoff

        [ServerRpc(RequireOwnership = false)]
        public void ForceTakeoffServerRPC()
        {
            ForceTakeoffClientRPC();
        }

        [ClientRpc]
        public void ForceTakeoffClientRPC()
        {
            StartOfRound.Instance.ShipLeaveAutomatically(false);
        }

        #endregion

        #region Bright Flashlight

        [ServerRpc(RequireOwnership = false)]
        public void FlashbrightServerRPC()
        {
            FlashbrightClientRPC();
        }

        [ClientRpc]
        public void FlashbrightClientRPC()
        {
            BrightFlashlight.IsEnabled = true;
        }

        #endregion

        #region Arachnophobia

        [ServerRpc(RequireOwnership = false)]
        public void ArachnophobiaServerRPC()
        {
            ArachnophobiaClientRPC();
            Arachnophobia.SpawnSpiders();
        }

        [ClientRpc]
        public void ArachnophobiaClientRPC()
        {
            Arachnophobia.IsEnabled = true;
        }

        #endregion
        

        #region Outside Coilhead

        [ServerRpc(RequireOwnership = false)]
        public void OutsideCoilheadServerRPC()
        {
            OutsideCoilhead.SpawnOutsideCoilhead();
        }

        public IEnumerator ServerDelayedCoilheadSetProperties(NetworkObjectReference netObj)
        {
            yield return new WaitForSeconds(5f);
            SetCoilheadNavmeshClientRPC(netObj.NetworkObjectId);
        }

        [ClientRpc]
        public void SetCoilheadNavmeshClientRPC(ulong netID)
        {
            foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
            {
                if (!(enemy is SpringManAI)) continue;

                if (enemy.NetworkObjectId == netID)
                {
                    OutsideCoilhead.SetNavmesh(enemy, true);
                    enemy.EnableEnemyMesh(true, false);
                }
            }
        }

        #endregion

        #region Moving mines

        [ServerRpc(RequireOwnership = false)]
        public void MovingMinesInitServerRPC()
        {
            MineOverflow.SpawnMoreMines(5);
            AddMovingMinesClientRPC();
        }

        [ClientRpc]
        public void AddMovingMinesClientRPC()
        {
            StartCoroutine(MovingFans.WaitForTrapInit(GetEnemies.SpawnableLandmine.prefabToSpawn.name));
        }

        [ServerRpc(RequireOwnership = false)]
        public void MovingBeartrapsServerRPC()
        {
            //MovingBeartraps.spawnBeartraps(Random.value < 0.5f);
            DynamicTrapEffect.spawnTrap(3, "GravelBeartrap", false);
            MovingBeartrapsClientRPC();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MovingCratesServerRPC()
        {
            StartCoroutine(MovingCrates.MakeMovingCrates());
        }

        [ServerRpc(RequireOwnership = false)]
        public void CrateOpenServerRPC(ulong netID)
        {
            StartCoroutine(MovingCrates.crateOpener(netID));
        }

        [ClientRpc]
        public void MovingBeartrapsClientRPC()
        {
            StartCoroutine(MovingFans.WaitForTrapInit("Beartrap"));
        }

        // IEnumerator WaitForMineInit()
        // {
        //     yield return new WaitForSeconds(5f);
        //     foreach (Landmine mine in GameObject.FindObjectsOfType<Landmine>())
        //     {
        //         if (mine.transform.parent.gameObject.GetComponent<LandmineMovement>() == null)
        //         {
        //             mine.transform.parent.gameObject.AddComponent<LandmineMovement>().LandmineScr = mine;
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// this is inefficient, but stays for now
        // /// </summary>
        // /// <param name="mineID"></param>
        // /// <param name="speed"></param>
        // /// <param name="currentPosition"></param>
        // /// <param name="syncedPaths"></param>
        // /// <param name="blockedid"></param>
        // [ClientRpc]
        // public void SyncDataClientRPC(ulong mineID, float speed, Vector3 currentPosition, Vector3 targetPosition, int blockedid)
        // {
        //     if (IsServer) return;
        //
        //     foreach (LandmineMovement mine in GameObject.FindObjectsOfType<LandmineMovement>())
        //     {
        //         if (mine.LandmineScr.NetworkObjectId != mineID) continue;
        //
        //         mine.transform.position = currentPosition;
        //         mine.TargetPosition = targetPosition;
        //         mine.MoveSpeed = speed;
        //         mine.BlockedID = blockedid;
        //         mine.CalculateNewPath();
        //     }
        //
        // }
        #endregion
        
        #region SuperFlinger
        [ServerRpc(RequireOwnership = false)]
        public void SuperFlingerServerRPC(int playerID)
        {
            Horseshootnt.spawnFlinger(playerID);
        }
        
        #endregion
        
        #region CleaningCrew
        [ServerRpc(RequireOwnership = false)]
        public void CleaningCrewServerRPC(int playerID)
        {
            CleaningCrew.spawnCrew(playerID);
        }
        #endregion
        
        #region MovingFans

        [ServerRpc(RequireOwnership = false)]
        public void MovingFansServerRPC()
        {
            CustomTrapServerRPC(6, GetEnemies.Fan.prefabToSpawn.name, false, false);
            AddMovingTrapClientRPC(GetEnemies.Fan.prefabToSpawn.name);
        }
        [ClientRpc]
        public void AddMovingTrapClientRPC(string trapName, bool follower = false, int playerFollowing = 0) 
        {
            StartCoroutine(MovingFans.WaitForTrapInit(trapName,follower,playerFollowing));
        }

        

        #endregion
        
        #region Moving Seatraps

        [ServerRpc(RequireOwnership = false)]
        public void MovingSeatrapsServerRPC()
        {
            SeaminesOutsideServerRPC();
            BerthaOutsideServerRPC(3);
            AddMovingSeatrapsClientRPC();
        }
        [ClientRpc]
        public void AddMovingSeatrapsClientRPC()
        {
            StartCoroutine(MovingFans.WaitForTrapInit("Bertha"));
            StartCoroutine(MovingFans.WaitForTrapInit("Seamine"));
        }
        
        #endregion

        #region Moving traps

        [ServerRpc(RequireOwnership = false)]
        public void MovingTrapsInitServerRPC()
        {
            TPTraps.SpawnTeleporterTraps(5);
            AddMovingTrapClientRPC(GetEnemies.SpawnableTP.prefabToSpawn.name);
        }
        #endregion

        #region HyperShake

        [ServerRpc(RequireOwnership = false)]
            public void HyperShakeServerRPC()
            {
                List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

                foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
                {
                    PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                    if (Misc.IsPlayerAliveAndControlled(player))
                        validPlayers.Add(player);
                }

                PlayerControllerB selectedPlayer = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];

                HyperShakeClientRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,selectedPlayer));
            }

            [ClientRpc]
            public void HyperShakeClientRPC(int playerID)
            {
                if (!Misc.isPlayerLocal(playerID)) return;

                HyperShake.ShakeData shakeData = new HyperShake.ShakeData();
                shakeData.Player = GameNetworkManager.Instance.localPlayerController;
                shakeData.NextShakeTimer = 0f;
                shakeData.ShakingTimer = 0f;
                HyperShake.ShakingData = shakeData;
            }

            #endregion

        #region Lever Shake

        [ServerRpc(RequireOwnership = false)]
        public void LeverShakeServerRPC()
        {
            LeverShake.ServerUse();
            LeverShakeClientRPC();
        }

        [ClientRpc]
        public void LeverShakeClientRPC()
        {
            LeverShake.ClientsUse();
        }

        #endregion

        #region Neck Break
        [ServerRpc(RequireOwnership = false)]
        public void NeckBreakRandomPlayerServerRpc(int player)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            NeckBreakRandomPlayerClientRpc(player);
        }

        [ClientRpc]
        public void NeckBreakRandomPlayerClientRpc(int playerId)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (Misc.isPlayerLocal(playerId))
            {
                NeckBreak.BreakNeck();
            }
        }
        #endregion
        
        #region Neck Spin
        [ServerRpc(RequireOwnership = false)]
        public void NeckSpinRandomPlayerServerRpc(int player)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            NeckSpinRandomPlayerClientRpc(player);
        }

        [ClientRpc]
        public void NeckSpinRandomPlayerClientRpc(int playerId)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (Misc.isPlayerLocal(playerId))
            {
                NeckSpin.SpinNeck();
            }
        }
        #endregion

        #region Egg Boots
        [ServerRpc(RequireOwnership = false)]
        public void EggBootsServerRpc(int player)
        {
            EggBootsClientRpc(player);
        }

        [ClientRpc]
        public void EggBootsClientRpc(int playerId)
        {
            if (StartOfRound.Instance.allPlayerScripts[playerId] == GameNetworkManager.Instance.localPlayerController)
            {
                EggBoots.eggBootsEnabled = true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EggBootsAllServerRpc()
        {
            EggBootsAllClientRpc();
        }

        [ClientRpc]
        public void EggBootsAllClientRpc()
        {
              EggBoots.eggBootsEnabled = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnExplodeEggServerRpc(int player)
        {
            EggBoots.SpawnAndExplodeEgg(player);
        }
        #endregion

        #region Egg Boots Two
        [ServerRpc(RequireOwnership = false)]
        public void EggBootsTwoServerRpc(int player)
        {
            EggBootsTwoClientRpc(player);
        }

        [ClientRpc]
        public void EggBootsTwoClientRpc(int playerId)
        {
            EggBoots.eggBootsEnabled = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnExplodeEggTwoServerRpc(int player)
        {
            EggBoots.SpawnAndExplodeEgg(player);
        }
        #endregion

        #region Random Store Item

        [ServerRpc(RequireOwnership = false)]
        public void RandomStoreItemServerRPC(int playerID)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            RandomStoreItem.SpawnItem(playerID);
        }
        [ServerRpc(RequireOwnership = false)]
        public void RandomStoreItemsServerRPC(int playerID, int totalItems)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            
            RandomGreatStoreItem.SpawnItem(playerID, totalItems);
        }


        #endregion

        #region Item Swap
        [ServerRpc(RequireOwnership = false)]
        public void ItemSwapServerRPC()
        {
            ItemSwapClientRPC();
        }

        [ClientRpc]
        public void ItemSwapClientRPC()
        {
            PlayerControllerB p1, p2;
            p1 = Misc.GetRandomAlivePlayer();
            p2 = Misc.GetRandomAlivePlayer();
            if (getValidPlayers().Count == 1) return;
            while (p1 == p2)
            {
                p1 = Misc.GetRandomAlivePlayer();
                p2 = Misc.GetRandomAlivePlayer();
            }
            ItemSwap.itemSwap(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,p1), Array.IndexOf(StartOfRound.Instance.allPlayerScripts,p2));
        }
        #endregion

        #region Golden Touch

        [ServerRpc(RequireOwnership = false)]
        public void GoldenTouchServerRPC()
        {
            var player = Array.IndexOf(StartOfRound.Instance.allPlayerScripts,Misc.GetRandomAlivePlayer());
            int g = GoldenTouch.GetRandomItem(player);
            GoldenTouchClientRPC(player, g);
        }
        [ClientRpc]
        public void GoldenTouchClientRPC(int playerID, int itemSlot)
        {
            PlayerControllerB player = Misc.GetPlayerByUserID(playerID);
            player.ItemSlots[itemSlot].SetScrapValue(player.ItemSlots[itemSlot].scrapValue * 2);
            player.ItemSlots[itemSlot].GetComponentInChildren<ScanNodeProperties>().scrapValue = player.ItemSlots[itemSlot].scrapValue;
        }
        #endregion

        #region Light Burden

        [ServerRpc(RequireOwnership = false)]
        public void LightBurdenServerRPC()
        {
            LightBurdenClientRPC();
        }

        [ClientRpc]
        public void LightBurdenClientRPC()
        {
            LightBurden.lessenWeight(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,Misc.GetRandomAlivePlayer()));
        }
        #endregion

        #region Item Duplicator

        [ServerRpc(RequireOwnership = false)]
        public void ItemDuplicatorServerRPC(int playerID)
        {
            ItemDuplicator.duplicateItems(playerID);
        }
        #endregion

        #region Battery Drain

        [ServerRpc(RequireOwnership = false)]
        public void BatteryDrainServerRPC()
        {
            BatteryDrainClientRPC();
        }

        [ClientRpc]
        public void BatteryDrainClientRPC()
        {
            BatteryDrain.removeCharge(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,Misc.GetRandomAlivePlayer()));
        }
        #endregion

        #region Heavy Burden

        [ServerRpc(RequireOwnership = false)]
        public void HeavyBurdenServerRPC()
        {
            HeavyBurdenClientRPC();
        }

        [ClientRpc]
        public void HeavyBurdenClientRPC()
        {
            HeavyBurden.increaseWeight(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,Misc.GetRandomAlivePlayer()));
        }
        #endregion

        //Commented out
        #region Become Small
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void BecomeSmallServerRPC(ulong userID)
        // {
        //     BecomeSmallClientRPC(userID);
        // }
        // [ClientRpc]
        // public void BecomeSmallClientRPC(ulong userID)
        // {
        //     SizeDifference.BecomeSmall(userID);
        // }
        // //DEAR LORD WHAT HAVE I DONE
        // [ServerRpc(RequireOwnership = false)]
        // public void AllPlayerUseServerRPC()
        // {
        //     AllPlayerUseClientRPC();
        // }
        // [ClientRpc]
        // public void AllPlayerUseClientRPC()
        // {
        //     BecomeSmallAllServerRPC(StartOfRound.Instance.localPlayerController.actualClientId);
        // }
        // [ServerRpc(RequireOwnership = false)]
        // public void BecomeSmallAllServerRPC(ulong userID)
        // {
        //     BecomeSmallAllClientRPC(userID);
        // }
        // [ClientRpc]
        // public void BecomeSmallAllClientRPC(ulong userID)
        // {
        //     SizeDifferenceForAll.BecomeSmall(userID);
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void fixSizeServerRPC(ulong userID)
        // {
        //     fixSizeClientRPC(userID);
        // }
        //
        // [ClientRpc]
        // public void fixSizeClientRPC(ulong userID)
        // {
        //     SizeDifference.fixSize(userID);
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void SizeSwitcherServerRPC()
        // {
        //     SizeSwitcherClientRPC();
        // }
        //
        // [ClientRpc]
        // public void SizeSwitcherClientRPC()
        // {
        //     SizeDifferenceSwitcher.StartSwitcher();
        // }
        //
         #endregion

        #region Drunk

        [ServerRpc(RequireOwnership = false)]
        public void DrunkServerRPC(int userID,bool All = false)
        {
            
            DrunkClientRPC(userID, All);
        }

        [ClientRpc]
        public void DrunkClientRPC(int userID, bool All)
        {
            if(All)DrunkForAll.startDrinking(userID);
            else Drunk.startDrinking(userID);
        }
        #endregion

        #region Reroll
        [ServerRpc(RequireOwnership = false)]
        public void RerollServerRPC(int userID)
        {
            Reroll.DiceScrap(userID);
        }
        [ServerRpc(RequireOwnership = false)]
        public void RerollAllServerRPC()
        {
            RerollAllClientRPC();
        }
        [ClientRpc]
        public void RerollAllClientRPC()
        {
            RerollServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,StartOfRound.Instance.localPlayerController));
        }

        #endregion

        #region AnythingGrenade
        [ServerRpc(RequireOwnership = false)]
        public void AnythingGrenadeServerRPC(int userID)
        {
            AnythingGrenade.Grenade(userID);
        }
        

        #endregion
        
        #region Tarot
        [ServerRpc(RequireOwnership = false)]
        public void TarotServerRPC(int userID)
        {
            TarotCards.TarotScrap(userID);
        }
        #endregion

        #region Ghosts
        [ServerRpc(RequireOwnership = false)]
        public void SpawnGhostsServerRPC()
        {
            Ghosts.SpawnGhosts();
        }
        #endregion

        #region NutcrackerOutside
        [ServerRpc(RequireOwnership = false)]
        public void SpawnNutcrackerOutsideServerRPC()
        {
            NutcrackerOutside.SpawnOutsideNutcracker();
        }
        #endregion

        #region GiveAllDice
        [ServerRpc(RequireOwnership = false)]
        public void GiveAllDiceServerRPC(int userID, int dice)
        {
            GiveAllDice.DiceScrap(userID);
        }

        #endregion

        #region PenaltyRolls

        [ServerRpc(RequireOwnership = false)]
        public void doOxydePenaltyServerRpc()
        {
            OxydePenalty.doPenalty();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void doPenaltyServerRPC(int amount)
        {
            NavMeshPenalty.doPenalty(amount);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpecialDetonateServerRPC()
        {
            if (StartOfRound.Instance is null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            validPlayers = getValidPlayers();
            if (validPlayers.Count == 0) return;
            PlayerControllerB theUnluckyOne = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
            SpecialDetonatePlayerClientRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,theUnluckyOne));
        }
        [ClientRpc]
        public void SpecialDetonatePlayerClientRPC(int clientID)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            var player = StartOfRound.Instance.allPlayerScripts[clientID];
            if(!Misc.isPlayerLocal(clientID)) return;
            if(!Misc.IsPlayerAliveAndControlled(player)) return;
            MysteryDice.sounds.TryGetValue("MineTrigger", out AudioClip clip);
            AudioSource.PlayClipAtPoint(clip, player.transform.position);
            StartCoroutine(SpawnExplosionAfterSFX(player.transform.position, 3, 10, 70, 10));
        }
        [ServerRpc(RequireOwnership = false)]
        public void GalDetonateServerRPC()
        {
            if (StartOfRound.Instance is null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            GalDetonatePlayerClientRPC();
        }
        
        [ClientRpc]
        public void GalDetonatePlayerClientRPC()
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            if (DiceGalAI.Instances.Count==0) return;
            foreach (var gal in DiceGalAI.Instances)
            {
                MysteryDice.sounds.TryGetValue("MineTrigger", out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, gal.transform.position);
                StartCoroutine(SpawnExplosionAfterSFX(gal.transform.position, 3, 10, 70, 10));
            }
            
            
        }
       

        #endregion

        [ServerRpc(RequireOwnership = false)]
        public void AllFlyServerRPC()
        {
            allFlyClientRPC();
        }

        [ClientRpc]
        public void allFlyClientRPC()
        {
            Fly.CanFly = true;
            Misc.SafeTipMessage("You can fly now!", "Double Tap Space to Fly!");
        }
    }
}

