using BepInEx;
using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Dice;
using MysteryDice.Effects;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static MysteryDice.Effects.MovingLandmines;

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
            if (DieBehaviour.LogEffectsToConsole)
                MysteryDice.CustomLogger.LogInfo($"[Debug] Player: {playerName} rolled {effectName}");
        }

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
                    DetonatePlayerClientRpc(PlayerIDToExplode);
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

            foreach (GameObject playerPrefab in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = playerPrefab.GetComponent<PlayerControllerB>();
                if (Misc.IsPlayerAliveAndControlled(player))
                    validPlayers.Add(player);
            }

            PlayerControllerB theUnluckyOne = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];
            StartDoomCountdown(theUnluckyOne.playerClientId);
        }

        [ClientRpc]
        public void DetonatePlayerClientRpc(ulong clientID)
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

            StartOfRound.Instance.ReviveDeadPlayers();
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
            player.health = 100;
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
        public void NeckBreakRandomPlayerServerRpc()
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

            NeckBreakRandomPlayerClientRpc(Misc.GetRandomAlivePlayer().playerClientId);
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

        #region Random Store Item

        [ServerRpc(RequireOwnership = false)]
        public void RandomStoreItemRpc(ulong playerID)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            RandomStoreItem.SpawnItem(playerID, getRandomBuyableItem());
        }
        
        public void RandomStoreItemsRpc(ulong playerID, int totalItems)
        {
            if (StartOfRound.Instance == null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            List<Item> items = new List<Item>();
            for (int i = 0; i < totalItems; i++)
            {
                items.Add(getRandomBuyableItem());
            }
            RandomGreatStoreItem.SpawnItem(playerID, items);
        }
        public Item getRandomBuyableItem()
        {
            Terminal terminal = GameObject.FindObjectOfType<Terminal>();
            int i = UnityEngine.Random.Range(0, terminal.buyableItemsList.Count());
            return terminal.buyableItemsList[i];
        }


        #endregion

    }
}

