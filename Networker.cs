using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using LCTarrotCard;
using LethalLib.Modules;
using MysteryDice.Dice;
using MysteryDice.Effects;
using MysteryDice.Patches;
using MysteryDice.Visual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using static MysteryDice.Effects.MovingLandmines;
using static UnityEngine.RectTransform;

namespace MysteryDice
{
    internal class Networker : NetworkBehaviour
    {
        public static Networker Instance;
        public static float RebelTimer = 0f;
        public static bool CoilheadIgnoreStares = false;
        public override void OnNetworkSpawn()
        {
            Instance = this;
            base.OnNetworkSpawn();

            if (IsServer) return;

            DieBehaviour.AllowedEffects.Clear();
            StartCoroutine(SyncRequest());
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
            RequestEffectConfigServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
            RequestConfigSyncServerRPC(GameNetworkManager.Instance.localPlayerController.playerClientId);
        }

        public override void OnNetworkDespawn()
        {
            StartOfRoundPatch.ResetSettingsShared();
            base.OnNetworkDespawn();
        }

        void FixedUpdate()
        {
            UpdateMineTimers();
            Armageddon.BoomTimer();
            HyperShake.FixedUpdate();
            LeverShake.FixedUpdate();
            Drunk.FixedUpdate();
            DrunkForAll.FixedUpdate();
        }
        void Update()
        {
            ModifyPitch.PitchFluctuate();
            RebelCoilheads();
            AlarmCurse.TimerUpdate();
        }


        [ServerRpc(RequireOwnership = false)]
        public void LogEffectsToOwnerServerRPC(string playerName, string effectName)
        {
            if (MysteryDice.debugDice.Value) MysteryDice.CustomLogger.LogInfo($"[Debug] Player: {playerName} rolled {effectName}");
            if (MysteryDice.debugChat.Value == MysteryDice.chatDebug.HostOnly) Misc.ChatWrite($"Player: {playerName} rolled {effectName}");
            if (MysteryDice.debugChat.Value == MysteryDice.chatDebug.Everyone) LogEffectsToEveryoneClientRPC(playerName, effectName);
        }
        [ClientRpc]
        public void LogEffectsToEveryoneClientRPC(string playerName, string effectName)
        {
            Misc.ChatWrite($"Player: {playerName} rolled {effectName}");
        }
        #region Config stuff
        [ServerRpc(RequireOwnership = false)]
        public void RequestEffectConfigServerRPC(ulong playerID)
        {
            foreach (var effect in DieBehaviour.AllowedEffects)
                SendConfigClientRPC(playerID, effect.Name);
        }
        [ClientRpc]
        public void SendConfigClientRPC(ulong playerID, string effectName)
        {
            if (IsServer) return;
            if (GameNetworkManager.Instance.localPlayerController.playerClientId == playerID)
            {
                DieBehaviour.AllowedEffects.Add(
                    DieBehaviour.AllEffects.Where(x => x.Name == effectName).First()
                );
            }
        }
        [ServerRpc(RequireOwnership = false)]
        public void RequestConfigSyncServerRPC(ulong playerID)
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
        public void SendConfigsClientRPC(ulong playerID, string key, string section, int type, int ival = 0, bool bval = false, string sval = "", string enumVal = "")
        {
            if (IsServer || GameNetworkManager.Instance.localPlayerController.playerClientId != playerID) return;

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
        #endregion
        
        #region Delay
        [ServerRpc]
        public void DelayedReactionServerRPC(ulong userID)
        {
            StartCoroutine(delayed(userID));
        }

        IEnumerator delayed(ulong userID)
        {
            UnityEngine.Random.Range(15, 45);
            yield return new WaitForSeconds(UnityEngine.Random.Range(15, 45));
            DelayedReactionClientRPC(userID);
        }

        [ClientRpc]
        public void DelayedReactionClientRPC(ulong userID)
        {
            Delay.DelayedReaction(userID);
        }
        #endregion

        #region Detonate
        private static Vector2 TimerRange = new Vector2(3f, 6f);
        private static ulong PlayerIDToExplode;
        private static float ExplosionTimer = 0f;



        public static bool IsPlayerAlive(PlayerControllerB player)
        {
            return !player.isPlayerDead &&
                    player.isActiveAndEnabled &&
                    player.IsSpawned;
        }

        public void UpdateMineTimers()
        {
            if (ExplosionTimer >= 0f)
            {
                ExplosionTimer -= Time.fixedDeltaTime;

                if (ExplosionTimer < 0f)
                    DetonatePlayerClientRPC(PlayerIDToExplode);
            }
        }
        public void StartDoomCountdown(ulong playerID)
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
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            validPlayers = getValidPlayers();

            PlayerControllerB theUnluckyOne = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
            StartDoomCountdown(theUnluckyOne.playerClientId);
        }

        public List<PlayerControllerB> getValidPlayers()
        {

            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (Misc.IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }
            return validPlayers;
        }

        [ClientRpc]
        public void DetonatePlayerClientRPC(ulong clientID)
        {
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (player.playerClientId == clientID &&
                    Misc.IsPlayerAliveAndControlled(player))
                {
                    AudioSource.PlayClipAtPoint(MysteryDice.MineSFX, player.transform.position);
                    StartCoroutine(SpawnExplosionAfterSFX(player.transform.position));
                    break;
                }
            }
        }

        IEnumerator SpawnExplosionAfterSFX(Vector3 position)
        {
            yield return new WaitForSeconds(0.5f);
            Landmine.SpawnExplosion(position, true, 1, 5, 50, 0, null, false);
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
                player.hasBegunSpectating = false;
                //HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                player.hinderedMultiplier = 1f;
                player.isMovementHindered = 0;
                player.sourcesCausingSinking = 0;
                HUDManager.Instance.HideHUD(false);
            }

        }
        [ServerRpc(RequireOwnership = false)]
        public void GiveLifeServerRpc(int num)
        {
            GiveLifeClientRpc(num);
        }

        [ClientRpc]
        public void GiveLifeClientRpc(int num)
        {
            Revive.lives += num;
            HUDManager.Instance.DisplayTip("Extra life", "You just got an extra life!");
        }

        #endregion

        #region TeleportInside

        [ServerRpc(RequireOwnership = false)]
        public void TeleportInsideServerRPC(ulong clientID, Vector3 teleportPos)
        {
            TeleportInsideClientRPC(clientID, teleportPos);
        }

        [ClientRpc]
        public void TeleportInsideClientRPC(ulong clientID, Vector3 teleportPos)
        {
            TeleportInside.TeleportPlayerInside(clientID, teleportPos);
        }
        #endregion

        #region TeleportToShip

        [ServerRpc(RequireOwnership = false)]
        public void TeleportToShipServerRPC(ulong clientID)
        {
            TeleportToShipClientRPC(clientID);
        }

        [ClientRpc]
        public void TeleportToShipClientRPC(ulong clientID)
        {
            ReturnToShip.TeleportPlayerToShip(clientID);
        }
        #endregion

        #region Teleport To Player

        [ServerRpc(RequireOwnership = false)]
        public void TeleportToPlayerServerRPC(ulong clientID, ulong to)
        {
            TeleportToPlayerClientRPC(clientID, to);
        }

        [ClientRpc]
        public void TeleportToPlayerClientRPC(ulong clientID, ulong to)
        {
            EveryoneToSomeone.TeleportPlayerToPlayer(clientID, to);
        }
        #endregion 

        #region EmergencyMeeting

        [ServerRpc(RequireOwnership = false)]
        public void EmergencyMeetingServerRPC()
        {
            EmergencyMeetingClientRPC();
            InteractTrigger doorButton = GameObject.Find(StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton").GetComponentInChildren<InteractTrigger>();
            doorButton.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);
            EmergencyMeeting.allEnemiesToShip();
            EmergencyAllClientRPC();
        }

        [ClientRpc]
        public void EmergencyMeetingClientRPC()
        {
            EmergencyMeeting.TeleportEveryoneToShip();
            
        }
        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerToShipServerRPC(ulong plyr)
        {
            TeleportPlayerToShipClientRPC(plyr);
        }

        [ClientRpc]
        public void TeleportPlayerToShipClientRPC(ulong plyr)
        {
            EmergencyMeeting.TeleportPlayerToShip(plyr);
        }
        [ClientRpc]
        public void EmergencyAllClientRPC()
        {
            MysteryDice.JumpscareScript.EmergencyMeeting();
        }
        #endregion

        #region playSound
        [ServerRpc(RequireOwnership =false)]
        public void PlaySoundServerRPC(string sound)
        {
            PlaySoundClientRPC(sound);
        }
        [ClientRpc]
        public void PlaySoundClientRPC(string sound)
        {
            SoundClipManager.playSound(sound);
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

        [ServerRpc(RequireOwnership = false)]
        public void TerminalLockoutServerRPC()
        {
            TerminalLockoutClientRPC();
        }

        [ClientRpc]
        public void TerminalLockoutClientRPC()
        {
            TerminalPatch.hideShowTerminal(true, GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
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

        #region TPOverflowOutside
        [ServerRpc(RequireOwnership = false)]
        public void TPOverflowOutsideServerRPC()
        {
            int MinesToSpawn = UnityEngine.Random.Range(TpOverflowOutside.MinMinesToSpawn, TpOverflowOutside.MaxMinesToSpawn + 1);
            TpOverflowOutside.SpawnTPOutside(MinesToSpawn);
        }
        #endregion

        #region SpikeOverflowOutside
        [ServerRpc(RequireOwnership = false)]
        public void SpikeOverflowOutsideServerRPC()
        {
            int MinesToSpawn = UnityEngine.Random.Range(SpikeOverflowOutside.MinMinesToSpawn, SpikeOverflowOutside.MaxMinesToSpawn + 1);
            SpikeOverflowOutside.SpawnSpikeOutside(MinesToSpawn);
        }
        #endregion

        #region CustomTrap
        [ServerRpc(RequireOwnership = false)]
        public void CustomTrapServerRPC(int max, string trap, bool inside)
        {
            DynamicTrapEffect.spawnTrap(max, trap, inside);
        }
        #endregion

        #region CratesOutside
        [ServerRpc(RequireOwnership = false)]
        public void CratesOutsideServerRPC()
        {
            int MinesToSpawn = UnityEngine.Random.Range(CratesOutside.MinMinesToSpawn, CratesOutside.MaxMinesToSpawn + 1);
            CratesOutside.SpawnCratesOutside(MinesToSpawn);
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
            int MinesToSpawn = UnityEngine.Random.Range(SeaminesOutside.MinMinesToSpawn, SeaminesOutside.MaxMinesToSpawn + 1);
            SeaminesOutside.SpawnSeaminesOutside(MinesToSpawn);
        }
        #endregion
        
        #region BerthaOutside
        [ServerRpc(RequireOwnership = false)]
        public void BerthaOutsideServerRPC()
        {
            BerthaOutside.SpawnBerthaOutside(1);
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
        public void SwapPlayersServerRPC(ulong userID)
        {
            List<PlayerControllerB> validPlayers = new List<PlayerControllerB>();
            PlayerControllerB callingPlayer = null;

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();

                if (Misc.IsPlayerAliveAndControlled(player) && player.playerClientId != userID)
                    validPlayers.Add(player);
                if (Misc.IsPlayerAliveAndControlled(player) && player.playerClientId == userID)
                    callingPlayer = player;
            }

            if (callingPlayer == null ||
                validPlayers.Count == 0)
                return;

            ulong randomPlayer = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)].playerClientId;
            Swap.SwapPlayers(callingPlayer.playerClientId, randomPlayer);
            SwapPlayerClientRPC(callingPlayer.playerClientId, randomPlayer);
        }

        [ClientRpc]
        public void SwapPlayerClientRPC(ulong userID, ulong otherUserID)
        {
            if (IsServer) return;
            Swap.SwapPlayers(userID, otherUserID);
        }
        #endregion

        #region ScrapJackpot
        [ServerRpc(RequireOwnership = false)]
        public void JackpotServerRPC(ulong userID, int amount)
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

        #region SameScrap
        [ServerRpc(RequireOwnership = false)]
        public void SameScrapServerRPC(ulong userID, int amount, string scrap, bool sneaky = false)
        {
            AllSameScrap.SameScrap(userID, amount, scrap, sneaky);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AllOfOneTPServerRPC(NetworkObjectReference[] netObjs, ulong playerID)
        {
            AllOfOneTPClientRPC(netObjs, playerID);
        }
        [ClientRpc]
        public void AllOfOneTPClientRPC(NetworkObjectReference[] netObjs, ulong playerID)
        {
            AllSameScrap.teleport(netObjs, playerID);
        }
        #endregion

        #region EggFountain
        [ServerRpc(RequireOwnership = false)]
        public void EggFountainServerRPC(ulong userID, int use)
        {
            EggFountain.spawnEggs(userID, use);
        }
        [ServerRpc(RequireOwnership = false)]
        public void explodeItemServerRPC(ulong objectId, bool egg, int count)
        {
            explodeItemClientRPC(objectId, egg, count);
        }

        [ClientRpc]
        public void explodeItemClientRPC(ulong objectId, bool egg, int count)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;

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
            if(player.health<100) player.health = 100;
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
        public void ReturnPlayerToShipServerRPC(ulong clientID)
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
        public void JumpscareAllServerRPC()
        {
            JumpscareAllClientRPC();
        }

        [ClientRpc]
        public void JumpscareAllClientRPC()
        {
            MysteryDice.JumpscareScript.Scare();
        }

        public IEnumerator DelayJumpscare()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 60f));
            JumpscareAllServerRPC();
        }
        #endregion

        #region AlarmCurse

        [ServerRpc(RequireOwnership = false)]
        public void AlarmCurseServerRPC(Vector3 position)
        {
            AlarmCurse.AlarmAudio(position);
            AlarmCurseClientRPC(position);
        }

        [ClientRpc]
        public void AlarmCurseClientRPC(Vector3 position)
        {
            if (IsServer) return;
            AlarmCurse.AlarmAudio(position);
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
        public void ZombieToShipServerRPC(ulong userID)
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

        #region Shotgun

        [ServerRpc(RequireOwnership = false)]
        public void ShotgunServerRPC(ulong playerID)
        {
            Shotgun.SpawnShotgun(playerID);
        }
        #endregion

        #region Pokeballs

        [ServerRpc(RequireOwnership = false)]
        public void PokeballsServerRPC(ulong playerID)
        {
            CatchEmAll.SpawnPokeballs(playerID);
        }
        [ServerRpc(RequireOwnership = false)]
        public void MasterballServerRPC(ulong playerID)
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
        public void PathfinderGiveSpawnerServerRPC(ulong playerID)
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
            StartCoroutine(WaitForMineInit());
        }

        IEnumerator WaitForMineInit()
        {
            yield return new WaitForSeconds(5f);
            foreach (Landmine mine in GameObject.FindObjectsOfType<Landmine>())
            {
                if (mine.transform.parent.gameObject.GetComponent<LandmineMovement>() == null)
                {
                    mine.transform.parent.gameObject.AddComponent<LandmineMovement>().LandmineScr = mine;
                }
            }
        }

        /// <summary>
        /// this is inefficient, but stays for now
        /// </summary>
        /// <param name="mineID"></param>
        /// <param name="speed"></param>
        /// <param name="currentPosition"></param>
        /// <param name="syncedPaths"></param>
        /// <param name="blockedid"></param>
        [ClientRpc]
        public void SyncDataClientRPC(ulong mineID, float speed, Vector3 currentPosition, Vector3 targetPosition, int blockedid)
        {
            if (IsServer) return;

            foreach (LandmineMovement mine in GameObject.FindObjectsOfType<LandmineMovement>())
            {
                if (mine.LandmineScr.NetworkObjectId != mineID) continue;

                mine.transform.position = currentPosition;
                mine.TargetPosition = targetPosition;
                mine.MoveSpeed = speed;
                mine.BlockedID = blockedid;
                mine.CalculateNewPath();
            }

        }
        #endregion

        #region Moving traps


            [ServerRpc(RequireOwnership = false)]
                public void MovingTrapsInitServerRPC()
                {
                    TPTraps.SpawnTeleporterTraps(5);
                    AddMovingTrapsClientRPC();
                }

                [ClientRpc]
                public void AddMovingTrapsClientRPC()
                {
                    StartCoroutine(WaitForTrapInit());
                }

                IEnumerator WaitForTrapInit()
                {
                    yield return new WaitForSeconds(5f);

                    foreach (var trap in GameObject.FindObjectsOfType<Component>().Where(c => c.GetType().Name == "TeleporterTrap"))
                    {
                        if (trap == null)
                        {
                            MysteryDice.CustomLogger.LogWarning("Trap is null.");
                            continue;
                        }

                        var targetObject = trap.transform.parent?.gameObject ?? trap.gameObject;

                        if (targetObject.GetComponent<MovingTPTraps.TeleporterTrapMovement>() == null)
                        {
                            var movementComponent = targetObject.AddComponent<MovingTPTraps.TeleporterTrapMovement>(); 
                            movementComponent.TeleporterTrapScr = trap;

                            if (movementComponent.TeleporterTrapScr == null)
                            {
                                MysteryDice.CustomLogger.LogWarning("Failed to set TeleporterTrapScr for trap: " + trap.name);
                            }
                            else
                            {
                                MysteryDice.CustomLogger.LogInfo("Successfully set TeleporterTrapScr for trap: " + trap.name);
                            }
                        }
                        else
                        {
                            MysteryDice.CustomLogger.LogInfo("Movement component already exists for trap: " + trap.name);
                        }
                    }
                }

                /// <summary>
                /// this is inefficient, but stays for now
                /// </summary>
                /// <param name="trapID"></param>
                /// <param name="speed"></param>
                /// <param name="currentPosition"></param>
                /// <param name="syncedPaths"></param>
                /// <param name="blockedid"></param>
                [ClientRpc]
                public void SyncDataTPClientRPC(ulong trapID, float speed, Vector3 currentPosition, Vector3 targetPosition, int blockedid)
                {
                    if (IsServer) return;

                    foreach (var trap in GameObject.FindObjectsOfType<MovingTPTraps.TeleporterTrapMovement>())
                    {
                        var networkObjectIdProp = trap.TeleporterTrapScr.GetType().GetProperty("NetworkObjectId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (networkObjectIdProp != null && (ulong)networkObjectIdProp.GetValue(trap.TeleporterTrapScr) != trapID) continue;

                        trap.transform.position = currentPosition;
                        trap.TargetPosition = targetPosition;
                        trap.MoveSpeed = speed;
                        trap.BlockedID = blockedid;
                        trap.CalculateNewPath();
                    }
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

                HyperShakeClientRPC(selectedPlayer.playerClientId);
            }

            [ClientRpc]
            public void HyperShakeClientRPC(ulong playerID)
            {
                if (GameNetworkManager.Instance.localPlayerController.playerClientId != playerID) return;

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
        public void NeckBreakRandomPlayerServerRpc(ulong player)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            NeckBreakRandomPlayerClientRpc(player);
        }

        [ClientRpc]
        public void NeckBreakRandomPlayerClientRpc(ulong playerId)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (playerId == GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                NeckBreak.BreakNeck();
            }
        }
        #endregion

        #region Neck Spin
        [ServerRpc(RequireOwnership = false)]
        public void NeckSpinRandomPlayerServerRpc(ulong player)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            NeckSpinRandomPlayerClientRpc(player);
        }

        [ClientRpc]
        public void NeckSpinRandomPlayerClientRpc(ulong playerId)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            if (playerId == GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                NeckSpin.SpinNeck();
            }
        }
        #endregion

        #region Egg Boots
        [ServerRpc(RequireOwnership = false)]
        public void EggBootsServerRpc(ulong player)
        {
            EggBootsClientRpc(player);
        }

        [ClientRpc]
        public void EggBootsClientRpc(ulong playerId)
        {
            if (playerId == GameNetworkManager.Instance.localPlayerController.playerClientId)
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
        public void spawnExplodeEggServerRpc(ulong player)
        {
            EggBoots.SpawnAndExplodeEgg(player);
        }
        #endregion

        #region Egg Boots Two
        [ServerRpc(RequireOwnership = false)]
        public void EggBootsTwoServerRpc(ulong player)
        {
            EggBootsTwoClientRpc(player);
        }

        [ClientRpc]
        public void EggBootsTwoClientRpc(ulong playerId)
        {
            EggBoots.eggBootsEnabled = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnExplodeEggTwoServerRpc(ulong player)
        {
            EggBoots.SpawnAndExplodeEgg(player);
        }
        #endregion

        #region Random Store Item

        [ServerRpc(RequireOwnership = false)]
        public void RandomStoreItemServerRPC(ulong playerID)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            RandomStoreItem.SpawnItem(playerID);
        }
        [ServerRpc(RequireOwnership = false)]
        public void RandomStoreItemsServerRPC(ulong playerID, int totalItems)
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
            ItemSwap.itemSwap(p1.playerClientId, p2.playerClientId);
        }
        #endregion

        #region Golden Touch

        [ServerRpc(RequireOwnership = false)]
        public void GoldenTouchServerRPC()
        {
            var player = Misc.GetRandomAlivePlayer().playerClientId;
            int g = GoldenTouch.GetRandomItem(player);
            GoldenTouchClientRPC(player, g);
        }
        [ClientRpc]
        public void GoldenTouchClientRPC(ulong playerID, int itemSlot)
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
            LightBurden.lessenWeight(Misc.GetRandomAlivePlayer().playerClientId);
        }
        #endregion

        #region Item Duplicator

        [ServerRpc(RequireOwnership = false)]
        public void ItemDuplicatorServerRPC(ulong playerID)
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
            BatteryDrain.removeCharge(Misc.GetRandomAlivePlayer().playerClientId);
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
            HeavyBurden.increaseWeight(Misc.GetRandomAlivePlayer().playerClientId);
        }
        #endregion

        #region Become Small

        [ServerRpc(RequireOwnership = false)]
        public void BecomeSmallServerRPC(ulong userID)
        {
            BecomeSmallClientRPC(userID);
        }
        [ClientRpc]
        public void BecomeSmallClientRPC(ulong userID)
        {
            SizeDifference.BecomeSmall(userID);
        }
        //DEAR LORD WHAT HAVE I DONE
        [ServerRpc(RequireOwnership = false)]
        public void AllPlayerUseServerRPC()
        {
            AllPlayerUseClientRPC();
        }
        [ClientRpc]
        public void AllPlayerUseClientRPC()
        {
            BecomeSmallAllServerRPC(StartOfRound.Instance.localPlayerController.playerClientId);
        }
        [ServerRpc(RequireOwnership = false)]
        public void BecomeSmallAllServerRPC(ulong userID)
        {
            BecomeSmallAllClientRPC(userID);
        }
        [ClientRpc]
        public void BecomeSmallAllClientRPC(ulong userID)
        {
            SizeDifferenceForAll.BecomeSmall(userID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void fixSizeServerRPC(ulong userID)
        {
            fixSizeClientRPC(userID);
        }
        [ClientRpc]
        public void fixSizeClientRPC(ulong userID)
        {
            SizeDifference.fixSize(userID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SizeSwitcherServerRPC()
        {
            SizeSwitcherClientRPC();
        }

        [ClientRpc]
        public void SizeSwitcherClientRPC()
        {
            SizeDifferenceSwitcher.StartSwitcher();
        }

        #endregion

        #region Drunk

        [ServerRpc(RequireOwnership = false)]
        public void DrunkServerRPC(ulong userID,bool All = false)
        {
            
            DrunkClientRPC(userID, All);
        }

        [ClientRpc]
        public void DrunkClientRPC(ulong userID, bool All)
        {
            if(All)DrunkForAll.startDrinking(userID);
            else Drunk.startDrinking(userID);
        }
        #endregion

        #region Reroll
        [ServerRpc(RequireOwnership = false)]
        public void RerollServerRPC(ulong userID)
        {
            Reroll.DiceScrap(userID);
        }

        #endregion

        #region AnythingGrenade
        [ServerRpc(RequireOwnership = false)]
        public void AnythingGrenadeServerRPC(ulong userID)
        {
            AnythingGrenade.Grenade(userID);
        }
        

        #endregion
        
        #region Tarot
        [ServerRpc(RequireOwnership = false)]
        public void TarotServerRPC(ulong userID)
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
        public void GiveAllDiceServerRPC(ulong userID, int dice)
        {
            GiveAllDice.DiceScrap(userID);
        }

        #endregion


    }
}

