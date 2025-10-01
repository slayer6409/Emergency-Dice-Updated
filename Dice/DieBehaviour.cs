using System;
using BepInEx.Configuration;
using MysteryDice.Effects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using KaimiraGames;
using MysteryDice.CompatThings;
using MysteryDice.Patches;
using MysteryDice.Visual;
using Random = UnityEngine.Random;

namespace MysteryDice.Dice
{
    public abstract class DieBehaviour : PhysicsProp
    {
        public static List<IEffect> AllEffects = new List<IEffect>();
        public static List<IEffect> CompleteEffects = new List<IEffect>();
        public static List<IEffect> AllowedEffects = new List<IEffect>();
        public static List<IEffect> 
            AwfulEffects = new List<IEffect>(), 
            GoodEffects = new List<IEffect>(), 
            GreatEffects = new List<IEffect>(), 
            MixedEffects = new List<IEffect>(), 
            BadEffects = new List<IEffect>();
        public static List<ConfigEntry<bool>> effectConfigs = new List<ConfigEntry<bool>>();
        public GameObject DiceModel;
        public List<IEffect> Effects = new List<IEffect>();
        private static List<string> surfacedNames = new List<string>();
        private static List<string> CodeRebirthNames = new List<string>();
        public static List<string> SteveNames = new List<string>();
        public List<IEffect> SurfacedEffects = new List<IEffect>();
        public List<IEffect> CodeRebirthEffects = new List<IEffect>();
        public List<IEffect> SteveEffects = new List<IEffect>();
        public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();
        public bool wasEnemy = false;
        public bool wasGhost = false;
        public bool wasCurse = false;
        public bool isRolling = false;

        public PlayerControllerB PlayerUser = null;
        public EnemyAI EnemyUser = null;
        public enum ShowEffect
        {
            ALL,
            NONE,
            DEFAULT,
            RANDOM
        } 
        public enum DiceType
        {
            CHRONOS,
            EMERGENCY,
            GAMBLER,
            RUSTY,
            SACRIFICER,
            SAINT,
            SURFACED,
            CODEREBIRTH,
            STEVE
        }
        public DiceType myType;
        public virtual void SetupDiceEffects()
        {
            Effects = new List<IEffect>(AllowedEffects);
            SurfacedEffects = AllowedEffects.Where(e=>surfacedNames.Contains(e.Name)).ToList();
            CodeRebirthEffects = AllowedEffects.Where(e=>CodeRebirthNames.Contains(e.Name)).ToList();
            SteveEffects = AllowedEffects.Where(e=>SteveNames.Contains(e.Name)).ToList();
        }
        public virtual void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Mixed });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }
        public override void Start()
        {
            base.Start();
            if(myType != DiceType.STEVE) DiceModel = gameObject.transform.Find("Model").gameObject;
            if(myType != DiceType.STEVE) DiceModel.AddComponent<Spinner>();
            SetupDiceEffects();
            SetupRollToEffectMapping();
            SetupDiceStuff();
            if (MysteryDice.aprilFoolsConfig.Value)
            {
                if (IsHost)
                {
                    StartCoroutine(waitResize());
                }
                this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1.5f, this.gameObject.transform.position.z);
                this.targetFloorPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1.5f, this.gameObject.transform.position.z);
            }
        }

        public IEnumerator waitResize()
        {
            yield return new WaitForSeconds(1f);
            var randomSize = new Vector3(Random.Range(0.4f, 1.7f), Random.Range(0.4f, 1.7f), Random.Range(0.4f, 1.7f));
            Networker.Instance.setDiceSizeClientRPC(this.NetworkObject.NetworkObjectId,randomSize);
        }
        
        public void SetupDiceStuff()
        {
            switch (myType)
            {
                case DiceType.CHRONOS:
                    DiceModel.AddComponent<ColorGradient>();
                    break;
                case DiceType.EMERGENCY:
                    DiceModel.AddComponent<Blinking>();
                    this.itemProperties.verticalOffset = 0;
                    break;
                case DiceType.GAMBLER:
                    DiceModel.AddComponent<CycleSigns>(); 
                    DiceModel.GetComponent<Spinner>().NewDice = true;
                    break;
                case DiceType.RUSTY:
                    DiceModel.GetComponent<Spinner>().NewDice = true;
                    break;
                case DiceType.SACRIFICER:
                    DiceModel.GetComponent<Spinner>().NewDice = true;
                    break;
                case DiceType.SAINT:
                    DiceModel.transform.Find("halo").gameObject.AddComponent<HaloSpin>();
                    break;
                case DiceType.SURFACED:
                    DiceModel = gameObject.transform.Find("Model").gameObject;
                    DiceModel.GetComponent<Spinner>().NewDice = true;
                    break;
                case DiceType.CODEREBIRTH:
                    DiceModel = gameObject.transform.Find("Model").gameObject;
                    DiceModel.GetComponent<Spinner>().NewDice = true;
                    break;
                case DiceType.STEVE:
                    DiceModel = gameObject.transform.Find("SteveLeLePoissonDé").gameObject;
                    //DiceModel.GetComponent<Spinner>().SurfacedDie = true;
                    break;
            }
            
        }
        public override void PocketItem()
        {
            if (DiceModel.GetComponent<Renderer>())
            {
                DiceModel.GetComponent<Renderer>().enabled = true;
            }
            DiceModel.SetActive(false);
            PlayerUser = this.playerHeldBy;
            base.PocketItem();

        }
        public override void EquipItem()
        {
            PlayerUser = this.playerHeldBy;
            DiceModel.SetActive(true);
            if (DiceModel.GetComponent<Renderer>() != null)
            {
                DiceModel.GetComponent<Renderer>().enabled = true;
            }
            base.EquipItem();
            
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            DiceModel.SetActive(true);
            if(PlayerUser==null) return;
            if(MysteryDice.toggleCursed.Value)
                if(Networker.Instance.cursedPeople.Contains(PlayerUser.playerSteamId) && Random.value < 0.6f) 
                    StartCoroutine(doStuff(8844199, PlayerUser));
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                if (StartOfRound.Instance == null) return;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
                if (playerHeldBy == null) return;
                PlayerUser = playerHeldBy;
                
                int dropperID = Array.IndexOf(StartOfRound.Instance.allPlayerScripts,playerHeldBy);
                GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GetItemFloorPosition(DiceModel.transform.parent.position), false);
                SyncDropServerRPC(dropperID, UnityEngine.Random.Range(0, 10));
            }
        }

        public IEnumerator doStuff(int dropperID, PlayerControllerB player)
        {
            if (StartOfRound.Instance == null) yield break;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) yield break;
            if (StartOfRound.Instance.localPlayerController != player) yield break;
            if (!IsHost) yield break;
            
            SyncDropServerRPC(dropperID, UnityEngine.Random.Range(0, 10));
        }
        
        public virtual IEnumerator UseTimer(int userID, int spinTime)
        { 
            if (isDestroyed || this == null || gameObject == null)
                yield break;
            try
            {
                if (!MysteryDice.randomSpinTime.Value) spinTime = 3;
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Starting Use Timer with Time: "+spinTime);
                if (myType!=DiceType.STEVE) DiceModel.GetComponent<Spinner>().StartHyperSpinning(spinTime);
            
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("After Hyperspin");
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError(e);
            }
            for (float t = 0; t < spinTime; t += Time.deltaTime)
            {
                yield return null;
            }
            try
            {
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("BeforeExplosion");
                if (MysteryDice.doDiceExplosion.Value && this != null && this.gameObject != null)
                {
                    Landmine.SpawnExplosion(this.gameObject.transform.position, true, 0f, 0f, 0, 0f, null, false);
                }
                if (userID == 6409046)
                {
                    wasEnemy = true;
                    if(MysteryDice.aprilFoolsConfig.Value) AprilFoolsRoll(); else Roll();
                }
                else if (userID == 7510501)
                {
                    wasGhost = true;
                    if (MysteryDice.aprilFoolsConfig.Value) AprilFoolsRoll();
                    else Roll();
                }
                else if (userID == 8844199)
                {
                    wasCurse = true;
                    if (MysteryDice.aprilFoolsConfig.Value) AprilFoolsRoll();
                    else Roll();
                }
                else
                {
                    if (Misc.isPlayerLocal(userID))
                    {
                        if (StartOfRound.Instance is null) yield break;
                        if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) yield break;

                        if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion" || StartOfRound.Instance.currentLevel.PlanetName == "98 Galetry" )
                        {
                            Misc.SafeTipMessage($"Company penalty", "Do not try this again.");
                            var effect = getRandomEffectByType(EffectType.Penalty, AllEffects);
                            effect.Use();
                        }
                        if (StartOfRound.Instance.currentLevel.sceneName == "Oxyde")
                        {
                            var effect = getRandomEffectByType(EffectType.SpecialPenalty, AllEffects);
                            effect.Use();
                        }
                        
                        if(MysteryDice.aprilFoolsConfig.Value) AprilFoolsRoll(); 
                        else Roll();
                    }
                }
                DestroyObject();
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError(e);
                DestroyObject();
            }
            yield return new WaitForSeconds(0.4f);
        }

        
        [ServerRpc(RequireOwnership = false)]
        public virtual void SyncDropServerRPC(int userID, int Timer)
        {
            isRolling = true;
            if (!IsHost) DropAndBlock(userID, Timer);

            SyncDropClientRPC(userID, Timer);
        }
        [ClientRpc]
        public virtual void SyncDropClientRPC(int userID, int Timer)
        {
            isRolling = true;
            DropAndBlock(userID, Timer);
        } 

        public virtual void DropAndBlock(int userID, int Timer)
        {
            if(userID==6409046) wasEnemy = true;
            if(userID==7510501) wasGhost = true;
            if(userID==8844199) wasCurse = true;
            grabbable = false;
            grabbableToEnemies = false;
            DiceModel.SetActive(true);
            StartOfRound.Instance.StartCoroutine(UseTimer(userID, Timer));
        } 
        private bool isDestroyed = false;
        public virtual void DestroyObject()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            grabbable = false;
            grabbableToEnemies = false;
            deactivated = true;
            // if (radarIcon != null)
            // {
            //     GameObject.Destroy(radarIcon.gameObject);
            // }
            // MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            // for (int i = 0; i < componentsInChildren.Length; i++)
            // {
            //     GameObject.Destroy(componentsInChildren[i]);
            // }
            // Collider[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Collider>();
            // for (int j = 0; j < componentsInChildren2.Length; j++)
            // {
            //     GameObject.Destroy(componentsInChildren2[j]);
            // }
            if (IsHost)
            {
                string parentName = this.gameObject.transform.parent != null
                    ? this.gameObject.transform.parent.gameObject.name
                    : "orphan :(";

                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug($"Despawning object: {this.NetworkObject.gameObject.name}\nParent Object: {parentName}");

                if (this.NetworkObject.gameObject.name.StartsWith("Networker"))
                {
                    MysteryDice.CustomLogger.LogDebug("Well there's your issue >:D");
                    return;
                }

                StartCoroutine(waitToDespawn());
            }
        }

        public IEnumerator waitToDespawn()
        {
            yield return new WaitForSeconds(1f);
            this.NetworkObject.Despawn();
        }
       
        public virtual void Roll()
        {
            if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Starting Roll");
            int diceRoll = UnityEngine.Random.Range(1, 7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            
            if(MysteryDice.DebugLogging.Value)MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
            randomEffect.Use();

            var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            ShowDefaultTooltip(randomEffect, diceRoll);
        }

        public static IEffect getRandomEffectByType(EffectType type, List<IEffect> effectsToUse)
        {
            List<IEffect> effectsChosen = new List<IEffect>();
            foreach (IEffect effect in effectsToUse)
            {
                if(effect.Outcome == type) effectsChosen.Add(effect);
            }
            return effectsChosen[UnityEngine.Random.Range(0, effectsChosen.Count)];
        }
        
        public IEffect GetRandomEffect(int diceRoll, List<IEffect> effects)
        {
            List<IEffect> rolledEffects = new List<IEffect>();
            if(effects.Count == 0) effects = new List<IEffect>(Effects);
            foreach (IEffect effect in effects)
                if (RollToEffect[diceRoll].Contains(effect.Outcome))
                    rolledEffects.Add(effect);

            if (rolledEffects.Count == 0) return null;
            int randomEffectID = UnityEngine.Random.Range(0, rolledEffects.Count);
            return rolledEffects[randomEffectID];
        }

        public void PlaySoundBasedOnEffect(EffectType effectType)
        {
            AudioClip clip;
            switch (effectType)
            {
                case EffectType.Awful:
                    MysteryDice.sounds.TryGetValue("Bell2", out clip);
                    break;
                case EffectType.Bad:
                    MysteryDice.sounds.TryGetValue("Bad1", out clip);
                    break;
                default:
                    MysteryDice.sounds.TryGetValue("Good2", out clip);
                    break;
            }

            AudioSource.PlayClipAtPoint(clip, GameNetworkManager.Instance.localPlayerController.transform.position);
        }
        public static void ShowDefaultTooltip(IEffect effect, int diceRoll)
        {
            string effectTypeMessage = "wat?";
            EffectType effectType = effect.Outcome;
            bool normalMessage = false;
            string message = "wat?";
            string[] awfulMessages = { ":)", ">:)", "Have fun!"};
            string[] badMessages = { "Welp", "This will be fun...", "Uh oh"};
            string[] mixedMessages = { "Debatable", "I honestly don't know if this is good or bad", "ehhh", "at least it's not bad"};
            string[] goodMessages = { "Enjoy.", "I like it", "Pretty good", "Yay!", "Woo!"};
            string[] greatMessages = { ";)", "I love it!", "Lucky!", "YAY!!!", "Only the best!", "♥♥♥" };

            switch (effectType)
            {
                case EffectType.Awful:
                    effectTypeMessage = awfulMessages[UnityEngine.Random.Range(0,awfulMessages.Length)];
                    break;
                case EffectType.Bad:
                    effectTypeMessage = badMessages[UnityEngine.Random.Range(0, badMessages.Length)];
                    break;
                case EffectType.Good:
                    effectTypeMessage = goodMessages[UnityEngine.Random.Range(0, goodMessages.Length)];
                    break;
                case EffectType.Great:
                    effectTypeMessage = greatMessages[UnityEngine.Random.Range(0, greatMessages.Length)];
                    break;
                case EffectType.Mixed:
                    effectTypeMessage = mixedMessages[UnityEngine.Random.Range(0, mixedMessages.Length)];
                    break;
            }

            var parsedEffect = DieBehaviour.ShowEffect.NONE;
            Enum.TryParse<DieBehaviour.ShowEffect>(MysteryDice.DisplayResults.Value, out parsedEffect);

            if (parsedEffect == ShowEffect.ALL)
            {
                normalMessage = true;
                message = effect.Tooltip;
            }
            else if (parsedEffect == ShowEffect.NONE)
            {
                message = effectTypeMessage;

            }
            else if (parsedEffect == ShowEffect.RANDOM)
            {
                int randint = UnityEngine.Random.Range(0, 101);
                if (randint <= 45)
                    message = effect.Tooltip;
                else
                    message = effectTypeMessage;
            }
            else if (parsedEffect == ShowEffect.DEFAULT)
            {
                if (effect.ShowDefaultTooltip)
                    message = effectTypeMessage;
                else
                    message = effect.Tooltip;
            }
            else
            {
                MysteryDice.CustomLogger.LogWarning("An error occured with DisplayResults The value is: " + MysteryDice.DisplayResults.Value);
                if (effect.ShowDefaultTooltip)
                    message = effectTypeMessage;
                else
                    message = effect.Tooltip;
            }

            Misc.SafeTipMessage($"Rolled {diceRoll}", message);
        }

        public static void Config()
        {
            
            MysteryDice.MainRegisterNewEffect(new NeckBreak());
            SteveNames.Add(new SelectEffect().Name);
            MysteryDice.MainRegisterNewEffect(new SelectEffect());
            SteveNames.Add(new Fly().Name);
            MysteryDice.MainRegisterNewEffect(new Fly());
            MysteryDice.MainRegisterNewEffect(new LeverShake());
            MysteryDice.MainRegisterNewEffect(new HyperShake());
            MysteryDice.MainRegisterNewEffect(new MovingLandmines());
            MysteryDice.MainRegisterNewEffect(new OutsideCoilhead());
            SteveNames.Add(new Arachnophobia().Name);
            MysteryDice.MainRegisterNewEffect(new Arachnophobia());
            MysteryDice.MainRegisterNewEffect(new BrightFlashlight());
            SteveNames.Add(new IncreasedRate().Name);
            MysteryDice.MainRegisterNewEffect(new IncreasedRate());
            MysteryDice.MainRegisterNewEffect(new DoorMalfunction());
            MysteryDice.MainRegisterNewEffect(new Purge());
            MysteryDice.MainRegisterNewEffect(new InfiniteStaminaAll());
            SteveNames.Add(new InfiniteStamina().Name);
            MysteryDice.MainRegisterNewEffect(new InfiniteStamina());
            MysteryDice.MainRegisterNewEffect(new Pathfinder());
            MysteryDice.MainRegisterNewEffect(new Shotgun());
            MysteryDice.MainRegisterNewEffect(new ShipTurret());
            MysteryDice.MainRegisterNewEffect(new TurretHell());
            MysteryDice.MainRegisterNewEffect(new SilentMine());
            MysteryDice.MainRegisterNewEffect(new ZombieToShip());
            MysteryDice.MainRegisterNewEffect(new InvertDoorLock());
            SteveNames.Add(new AlarmCurse().Name);
           
            
            MysteryDice.MainRegisterNewEffect(new AlarmCurse());
            SteveNames.Add(new JumpscareGlitch().Name);
            MysteryDice.MainRegisterNewEffect(new JumpscareGlitch());
            MysteryDice.MainRegisterNewEffect(new Armageddon());
            MysteryDice.MainRegisterNewEffect(new Beepocalypse());
            MysteryDice.MainRegisterNewEffect(new RebeliousCoilHeads());
            MysteryDice.MainRegisterNewEffect(new TurnOffLights());
            SteveNames.Add(new HealAndRestore().Name);
            MysteryDice.MainRegisterNewEffect(new HealAndRestore());
            MysteryDice.MainRegisterNewEffect(new ScrapJackpot());
            MysteryDice.MainRegisterNewEffect(new Swap());
            MysteryDice.MainRegisterNewEffect(new ModifyPitch());
            MysteryDice.MainRegisterNewEffect(new MineOverflow());
            MysteryDice.MainRegisterNewEffect(new MineOverflowOutside());
            SteveNames.Add(new InstaJester().Name);
            MysteryDice.MainRegisterNewEffect(new InstaJester());
            MysteryDice.MainRegisterNewEffect(new FakeFireExits());
            MysteryDice.MainRegisterNewEffect(new FireExitBlock());
            MysteryDice.MainRegisterNewEffect(new ReturnToShip());
            SteveNames.Add(new ReturnToShipTogether().Name);
            MysteryDice.MainRegisterNewEffect(new ReturnToShipTogether());
            SteveNames.Add(new TeleportInside().Name);
            MysteryDice.MainRegisterNewEffect(new TeleportInside());
            MysteryDice.MainRegisterNewEffect(new BugPlague());
            MysteryDice.MainRegisterNewEffect(new ZombieApocalypse());
            MysteryDice.MainRegisterNewEffect(new Revive());
            MysteryDice.MainRegisterNewEffect(new Detonate());
            MysteryDice.MainRegisterNewEffect(new RandomStoreItem());
            MysteryDice.MainRegisterNewEffect(new RandomGreatStoreItem());
            MysteryDice.MainRegisterNewEffect(new BatteryDrain());
            SteveNames.Add(new EveryoneToSomeone().Name);
            MysteryDice.MainRegisterNewEffect(new EveryoneToSomeone());
            MysteryDice.MainRegisterNewEffect(new LightBurden());
            MysteryDice.MainRegisterNewEffect(new ItemDuplicator());
            MysteryDice.MainRegisterNewEffect(new HeavyBurden());
            MysteryDice.MainRegisterNewEffect(new DrunkForAll(),false,true);
            MysteryDice.MainRegisterNewEffect(new Drunk());
            MysteryDice.MainRegisterNewEffect(new GoldenTouch());
            MysteryDice.MainRegisterNewEffect(new Reroll());
            MysteryDice.MainRegisterNewEffect(new NeckSpin());
            SteveNames.Add(new GiveAllDice().Name);
            MysteryDice.MainRegisterNewEffect(new GiveAllDice());
            MysteryDice.MainRegisterNewEffect(new InsideGiant());
            MysteryDice.MainRegisterNewEffect(new OutsideBugs());
            MysteryDice.MainRegisterNewEffect(new Maneaters());
            MysteryDice.MainRegisterNewEffect(new Delay());
            MysteryDice.MainRegisterNewEffect(new SpikeOverflowOutside(),true);
            MysteryDice.MainRegisterNewEffect(new Lasso());
            MysteryDice.MainRegisterNewEffect(new Meteors());
            MysteryDice.MainRegisterNewEffect(new Barbers());
            MysteryDice.MainRegisterNewEffect(new InvisibleEnemy());
            SteveNames.Add(new EmergencyMeeting().Name);
            MysteryDice.MainRegisterNewEffect(new EmergencyMeeting());
            MysteryDice.MainRegisterNewEffect(new Ghosts());
            MysteryDice.MainRegisterNewEffect(new MerryChristmas());
            SteveNames.Add(new NutcrackerOutside().Name);
            MysteryDice.MainRegisterNewEffect(new NutcrackerOutside());
            MysteryDice.MainRegisterNewEffect(new AllSameScrap());
            MysteryDice.MainRegisterNewEffect(new Eggs());
            MysteryDice.MainRegisterNewEffect(new EggFountain());
            MysteryDice.MainRegisterNewEffect(new FlashFountain());
            MysteryDice.MainRegisterNewEffect(new InsideDog());
            SteveNames.Add(new EggBoots().Name);
            MysteryDice.MainRegisterNewEffect(new EggBoots());
            MysteryDice.MainRegisterNewEffect(new EggBootsForAll(),false,true);
            MysteryDice.MainRegisterNewEffect(new RerollALL(),false,true);
            MysteryDice.MainRegisterNewEffect(new TulipTrapeze());
            SteveNames.Add(new BlameGlitch().Name);
            MysteryDice.MainRegisterNewEffect(new BlameGlitch(),true);
            MysteryDice.MainRegisterNewEffect(new HappyDay());
            MysteryDice.MainRegisterNewEffect(new SpicyNuggies());
            SteveNames.Add(new Martyrdom().Name);
            MysteryDice.MainRegisterNewEffect(new Martyrdom());
            SteveNames.Add(new Lovers().Name);
            MysteryDice.MainRegisterNewEffect(new Lovers());
            MysteryDice.MainRegisterNewEffect(new BIGSpike(), true);
            MysteryDice.MainRegisterNewEffect(new SpeedUp(),true, true);
            MysteryDice.MainRegisterNewEffect(new GiveCredits());
            MysteryDice.MainRegisterNewEffect(new RemoveCredits());
            MysteryDice.MainRegisterNewEffect(new BigDelivery());
            MysteryDice.MainRegisterNewEffect(new DorjesDream());
            SteveNames.Add(new MoreLives().Name);
            MysteryDice.MainRegisterNewEffect(new MoreLives());
            MysteryDice.MainRegisterNewEffect(new LizzieDog());
            MysteryDice.MainRegisterNewEffect(new WhereDidMyFriendsGo());
            SteveNames.Add(new Confusion().Name);
            MysteryDice.MainRegisterNewEffect(new Confusion());
            MysteryDice.MainRegisterNewEffect(new StupidConfusion(), true);
            SteveNames.Add(new NutsBaby().Name);
            MysteryDice.MainRegisterNewEffect(new NutsBaby());
            MysteryDice.MainRegisterNewEffect(new Papercut(), true);
            SteveNames.Add(new BadLovers().Name);
            MysteryDice.MainRegisterNewEffect(new BadLovers());
            MysteryDice.MainRegisterNewEffect(new OopsAllBlank());
            MysteryDice.MainRegisterNewEffect(new AllAtOnce(), true, true);
            MysteryDice.MainRegisterNewEffect(new Unkillable());
            MysteryDice.MainRegisterNewEffect(new FreebirdEnemy(), true);
            MysteryDice.MainRegisterNewEffect(new DoubleTrouble());
            SteveNames.Add(new ThreesCompany().Name);
            MysteryDice.MainRegisterNewEffect(new ThreesCompany());
            MysteryDice.MainRegisterNewEffect(new FreebirdTrap(), true);
            MysteryDice.MainRegisterNewEffect(new EvilFreebirdEnemy());
            MysteryDice.MainRegisterNewEffect(new MicroTrap(), true);
            SteveNames.Add(new ManyAds().Name);
            MysteryDice.MainRegisterNewEffect(new ManyAds());
            SteveNames.Add(new TulipBombers().Name);
            MysteryDice.MainRegisterNewEffect(new TulipBombers());
            SteveNames.Add(new MadScience().Name);
            MysteryDice.MainRegisterNewEffect(new MadScience());
            MysteryDice.MainRegisterNewEffect(new MadderScience());
            MysteryDice.MainRegisterNewEffect(new SpecialDetonate(), true);
            MysteryDice.MainRegisterNewEffect(new GalDetonate());
            MysteryDice.MainRegisterNewEffect(new AllFly());
            MysteryDice.MainRegisterNewEffect(new AddLife());
            MysteryDice.MainRegisterNewEffect(new Overweight());
            MysteryDice.MainRegisterNewEffect(new HouseAlwaysWins());
            MysteryDice.MainRegisterNewEffect(new MovingCrane());
            MysteryDice.MainRegisterNewEffect(new Lizard());
            MysteryDice.MainRegisterNewEffect(new MineExploder());
            MysteryDice.MainRegisterNewEffect(new TurretExploder());
            MysteryDice.MainRegisterNewEffect(new GlitchPill());
            MysteryDice.MainRegisterNewEffect(new JobAppPill());
            MysteryDice.MainRegisterNewEffect(new ChangePlaces());
            MysteryDice.MainRegisterNewEffect(new ReviveNext());
            MysteryDice.MainRegisterNewEffect(new RevivePercent());

            if (MysteryDice.TooManyEmotesPresent)
            {
                MysteryDice.MainRegisterNewEffect(new PerformRandomEmote());
            }
            
            if (MysteryDice.lethalThingsPresent)
            {
                MysteryDice.MainRegisterNewEffect(new TPTraps());
                MysteryDice.MainRegisterNewEffect(new MovingTPTraps());
                MysteryDice.MainRegisterNewEffect(new TpOverflowOutside());
                MysteryDice.MainRegisterNewEffect(new SilentTP());
                MysteryDice.MainRegisterNewEffect(new Friends());
            }
            if (MysteryDice.LethalMonPresent) 
            {
                MysteryDice.MainRegisterNewEffect(new CatchEmAll());
                MysteryDice.MainRegisterNewEffect(new LegendaryCatch());
            }
            
            if (MysteryDice.LCOfficePresent)
            {
                MysteryDice.MainRegisterNewEffect(new InsideShrimps());
            }
            if (MysteryDice.SurfacedPresent)
            {
                surfacedNames.Add(new Martyrdom().Name);
                surfacedNames.Add(new MantisShrimps().Name);
                MysteryDice.MainRegisterNewEffect(new MantisShrimps());
                surfacedNames.Add(new SeaminesOutside().Name);
                MysteryDice.MainRegisterNewEffect(new SeaminesOutside());
                surfacedNames.Add(new BerthaOutside().Name);
                MysteryDice.MainRegisterNewEffect(new BerthaOutside());
                surfacedNames.Add(new Bruce().Name);
                MysteryDice.MainRegisterNewEffect(new Bruce());
                surfacedNames.Add(new Nemo().Name);
                MysteryDice.MainRegisterNewEffect(new Nemo());
                surfacedNames.Add(new BellCrabs().Name);
                MysteryDice.MainRegisterNewEffect(new BellCrabs());
                surfacedNames.Add(new UrchinIndoors().Name);
                MysteryDice.MainRegisterNewEffect(new UrchinIndoors());
                surfacedNames.Add(new MineHardPlace().Name);
                SteveNames.Add(new MineHardPlace().Name);
                MysteryDice.MainRegisterNewEffect(new MineHardPlace());
                surfacedNames.Add(new Flinger().Name);
                MysteryDice.MainRegisterNewEffect(new Flinger());
                surfacedNames.Add(new BurgerFlippers().Name);
                MysteryDice.MainRegisterNewEffect(new BurgerFlippers());
                surfacedNames.Add(new ScaryMon().Name);
                MysteryDice.MainRegisterNewEffect(new ScaryMon());
                surfacedNames.Add(new BerthaLever().Name);
                MysteryDice.MainRegisterNewEffect(new BerthaLever());
                surfacedNames.Add(new InstantExplodingBerthas().Name);
                MysteryDice.MainRegisterNewEffect(new InstantExplodingBerthas());
                surfacedNames.Add(new BigJumpscare().Name);
                MysteryDice.MainRegisterNewEffect(new BigJumpscare());
                surfacedNames.Add(new BIGBertha().Name);
                SteveNames.Add(new BIGBertha().Name);
                MysteryDice.MainRegisterNewEffect(new BIGBertha());
                surfacedNames.Add(new MovingSeaTraps().Name);
                MysteryDice.MainRegisterNewEffect(new MovingSeaTraps());
                surfacedNames.Add(new BigFling().Name);
                MysteryDice.MainRegisterNewEffect(new BigFling());
                surfacedNames.Add(new SuperFlinger().Name);
                MysteryDice.MainRegisterNewEffect(new SuperFlinger());
                surfacedNames.Add(new Horseshootnt().Name);
                SteveNames.Add(new Horseshootnt().Name);
                MysteryDice.MainRegisterNewEffect(new Horseshootnt());
                surfacedNames.Add(new Bruiser().Name);
                MysteryDice.MainRegisterNewEffect(new Bruiser());
                surfacedNames.Add(new HorseshootSeat().Name);
                MysteryDice.MainRegisterNewEffect(new HorseshootSeat());
                surfacedNames.Add(new Rigo().Name);
                MysteryDice.MainRegisterNewEffect(new Rigo());
                surfacedNames.Add(new Nemosplosion().Name);
                MysteryDice.MainRegisterNewEffect(new Nemosplosion());
                surfacedNames.Add(new LongBertha().Name);
                SteveNames.Add(new LongBertha().Name);
                MysteryDice.MainRegisterNewEffect(new LongBertha());

                if (MysteryDice.CodeRebirthPresent)
                {
                    //     surfacedNames.Add(new MicrowaveBertha().Name);
                    //     MysteryDice.MainRegisterNewEffect(new MicrowaveBertha());
                }
            }
            if (MysteryDice.LCTarotCardPresent)
            {
                MysteryDice.MainRegisterNewEffect(new TarotCards());
            }
            if (MysteryDice.TakeyPlushPresent)
            {
                SteveNames.Add(new TakeySmol().Name);
                MysteryDice.MainRegisterNewEffect(new TakeySmol());
            }
            
            if (MysteryDice.CodeRebirthPresent) 
            {
                if (CodeRebirthCheckConfigs.getEnemy("Tornado")!= null)
                {
                    CodeRebirthNames.Add(new Tornado().Name);
                    MysteryDice.MainRegisterNewEffect(new Tornado());
                }

                if (CodeRebirthCheckConfigs.getTrap("Crate")!= null)
                {
                    CodeRebirthNames.Add(new CratesOutside().Name);
                    MysteryDice.MainRegisterNewEffect(new CratesOutside()); 
                    CodeRebirthNames.Add(new CratesInside().Name);
                    MysteryDice.MainRegisterNewEffect(new CratesInside());
                    CodeRebirthNames.Add(new MovingCrates().Name);
                    SteveNames.Add(new MovingCrates().Name);
                    MysteryDice.MainRegisterNewEffect(new MovingCrates());
                    CodeRebirthNames.Add(new IFeelSafe().Name);
                    SteveNames.Add(new IFeelSafe().Name);
                    MysteryDice.MainRegisterNewEffect(new IFeelSafe());
                }

                if (CodeRebirthCheckConfigs.getTrap("Industrial Fan")!= null)
                {
                    CodeRebirthNames.Add(new Fans().Name);
                    MysteryDice.MainRegisterNewEffect(new Fans());
                    CodeRebirthNames.Add(new MovingFans().Name);
                    MysteryDice.MainRegisterNewEffect(new MovingFans()); 
                    CodeRebirthNames.Add(new FollowerFan().Name);
                    MysteryDice.MainRegisterNewEffect(new FollowerFan());
                    if (CodeRebirthCheckConfigs.getTrap("Flash Turret")!= null)
                    {
                        SteveNames.Add(new Paparazzi().Name);
                        CodeRebirthNames.Add(new Paparazzi().Name); 
                        MysteryDice.MainRegisterNewEffect(new Paparazzi());
                    }
                }

                if (CodeRebirthCheckConfigs.getTrap("Flash Turret")!= null)
                {
                    CodeRebirthNames.Add(new Flashers().Name);
                    MysteryDice.MainRegisterNewEffect(new Flashers());
                    CodeRebirthNames.Add(new Bald().Name);
                    SteveNames.Add(new Bald().Name);
                    MysteryDice.MainRegisterNewEffect(new Bald());
                }
                if (CodeRebirthCheckConfigs.getTrap("Functional Microwave")!= null)
                {
                    CodeRebirthNames.Add(new Microwave().Name);
                    MysteryDice.MainRegisterNewEffect(new Microwave());
                }
                if (CodeRebirthCheckConfigs.getEnemy("Transporter")!= null)
                {
                    CodeRebirthNames.Add(new FreebirdJimothy().Name);
                    SteveNames.Add(new FreebirdJimothy().Name);
                    MysteryDice.MainRegisterNewEffect(new FreebirdJimothy());
                }
                if (CodeRebirthCheckConfigs.getEnemy("Janitor")!= null && CodeRebirthCheckConfigs.getEnemy("Transporter")!= null)
                {
                    SteveNames.Add(new CleaningCrew().Name);
                    CodeRebirthNames.Add(new CleaningCrew().Name);
                    MysteryDice.MainRegisterNewEffect(new CleaningCrew());
                }   
                if (CodeRebirthCheckConfigs.getEnemy("Redwood Titan")!= null)
                {
                     CodeRebirthNames.Add(new TheRumbling().Name);
                                    MysteryDice.MainRegisterNewEffect(new TheRumbling());
                }

                if (CodeRebirthCheckConfigs.getTrap("Autonomous Crane") != null)
                {
                    CodeRebirthNames.Add(new SmolCrane().Name);
                    MysteryDice.MainRegisterNewEffect(new SmolCrane());
                    CodeRebirthNames.Add(new BeegCrane().Name);
                    MysteryDice.MainRegisterNewEffect(new BeegCrane());
                }
                if (CodeRebirthCheckConfigs.getTrap("Emerging Cactus 1") != null)
                {
                    CodeRebirthNames.Add(new TheDesert().Name);
                    MysteryDice.MainRegisterNewEffect(new TheDesert());
                }
              
                MysteryDice.MainRegisterNewEffect(new OxydePenalty(), true);
                CodeRebirthNames.Add(new Zortin().Name);
                SteveNames.Add(new Zortin().Name);
                   MysteryDice.MainRegisterNewEffect(new Zortin());
                CodeRebirthNames.Add(new ZortinTwo().Name);
                SteveNames.Add(new ZortinTwo().Name);
                    MysteryDice.MainRegisterNewEffect(new ZortinTwo());
                CodeRebirthNames.Add(new BlameGlitch().Name);
                if (CodeRebirthCheckConfigs.getEnemy("Nancy") != null)
                {
                    CodeRebirthNames.Add(new Healers().Name);
                    MysteryDice.MainRegisterNewEffect(new Healers());
                }
                if (CodeRebirthCheckConfigs.getEnemy("Mistress") != null)
                {
                    CodeRebirthNames.Add(new ManyMistress().Name);
                    MysteryDice.MainRegisterNewEffect(new ManyMistress());
                } 
                if (CodeRebirthCheckConfigs.getEnemy("Guardsman") != null)
                {
                    CodeRebirthNames.Add(new GuardsmanOutside().Name);
                    MysteryDice.MainRegisterNewEffect(new GuardsmanOutside());
                } 
                if (CodeRebirthCheckConfigs.getEnemy("Peace Keeper") != null)
                {
                    CodeRebirthNames.Add(new PeaceTreaty().Name);
                    MysteryDice.MainRegisterNewEffect(new PeaceTreaty());
                }
                if (CodeRebirthCheckConfigs.getEnemy("Mistress") != null &&
                    CodeRebirthCheckConfigs.getEnemy("Rabbit Magician") != null &&
                    CodeRebirthCheckConfigs.getEnemy("Lord Of The Manor") != null)
                {
                    CodeRebirthNames.Add(new HappyFamily().Name);
                    MysteryDice.MainRegisterNewEffect(new HappyFamily());
                }
                if (CodeRebirthCheckConfigs.getEnemy("Real Enemy SnailCat") != null)
                {
                    CodeRebirthNames.Add(new ImmortalSnailcat().Name);
                    MysteryDice.MainRegisterNewEffect(new ImmortalSnailcat());
                }

                if (CodeRebirthCheckConfigs.getEnemy("Lord Of The Manor") != null)
                {
                    CodeRebirthNames.Add(new EtTuBrute().Name);
                    MysteryDice.MainRegisterNewEffect(new EtTuBrute());
                }
                if (CodeRebirthCheckConfigs.getEnemy("CutieFly") != null)
                {
                    CodeRebirthNames.Add(new HeatSeakingCutieFly().Name);
                    MysteryDice.MainRegisterNewEffect(new HeatSeakingCutieFly());
                }
            }
            
            if (MysteryDice.DiversityPresent)
            {
                if (DiversityEffect1.checkConfigs()) MysteryDice.MainRegisterNewEffect(new DiversityEffect1());
                if (DiversityEffect2.checkConfigs()) MysteryDice.MainRegisterNewEffect(new DiversityEffect2());
            }
            if (MysteryDice.BombCollarPresent) 
            {
                MysteryDice.MainRegisterNewEffect(new BombCollars());
                MysteryDice.MainRegisterNewEffect(new EveryoneFriends());
            }

            if (MysteryDice.NavMeshInCompanyPresent)
            {
                MysteryDice.MainRegisterNewEffect(new NavMeshPenalty(), true);
            }
            List<IEffect> sortedList = AllEffects.OrderBy(o => o.Name).ToList();
            CompleteEffects.AddRange(AllEffects);
            CompleteEffects = CompleteEffects.OrderBy(o => o.Name).ToList();
            AllEffects = sortedList;
        }

        #region AprilFools

        public void setupChronosRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed, EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Bad, EffectType.Good, EffectType.Great });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good, EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            doChronosRoll();
        }
        public void doChronosRoll()
        {
            try
            {
            float offset = TimeOfDay.Instance.normalizedTimeOfDay;
            WeightedList<int> weightedRolls = new WeightedList<int>();
            if (!MysteryDice.chronosUpdatedTimeOfDay.Value) 
            {
                weightedRolls.Add(1, 1 + (int)(offset * 10f));
                weightedRolls.Add(2, 1 + (int)(offset * 8f));
                weightedRolls.Add(3, 1 + (int)(offset * 6f));
                weightedRolls.Add(4, 1 + (int)(offset * 3f));
                weightedRolls.Add(5, 1 + (int)(offset * 1f));
                weightedRolls.Add(6, 1);
            }
            else if (MysteryDice.chronosUpdatedTimeOfDay.Value)
            {
                if(offset < .5f)
                {

                    weightedRolls.Add(1, 1 + (int)((1 - offset) * 10f));
                    weightedRolls.Add(2, 1 + (int)((1 - offset) * 8f));
                    weightedRolls.Add(3, 1 + (int)((1 - offset) * 6f));
                    weightedRolls.Add(4, 1 + (int)(offset * 4f));
                    weightedRolls.Add(5, 1 + (int)(offset * 2f));
                    weightedRolls.Add(6, 1 + (int)offset);
                }
                else if(offset >= .5f)
                {
                    weightedRolls.Add(1, 1 + (int)(offset * 4f));
                    weightedRolls.Add(2, 1 + (int)(offset * 2f));
                    weightedRolls.Add(3, 1 + (int)offset);
                    weightedRolls.Add(4, 1 + (int)((1 - offset) * 6f));
                    weightedRolls.Add(5, 1 + (int)((1 - offset) * 8f));
                    weightedRolls.Add(6, 1 + (int)((1 - offset) * 10f));
                }
            }

            bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

            int diceRoll = weightedRolls.Next();

            if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
            randomEffect.Use();

            
            var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            if (isOutside && !MysteryDice.useDiceOutside.Value)
            {
                Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                return;
            }
            ShowDefaultTooltip(randomEffect, diceRoll);

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
        }

        public void setupEmergencyRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            doEmergencyRoll();
        }
        public void doEmergencyRoll()
        {
            try
            {
                int diceRoll = UnityEngine.Random.Range(1, 7);

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

            
                PlaySoundBasedOnEffect(randomEffect.Outcome);

                if (diceRoll > 2) randomEffect = new ReturnToShip();
                if (diceRoll == 6) randomEffect = new ReturnToShipTogether();

                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            
                ShowDefaultTooltip(randomEffect, diceRoll);

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
                (new ReturnToShip()).Use();
            }
        }

        public void setupGamblerRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad, EffectType.Mixed });
            RollToEffect.Add(4, new EffectType[] { EffectType.Mixed, EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            doGamblerRoll();
        }
        public void doGamblerRoll()
        {
            try
            {
                bool isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                int diceRoll = UnityEngine.Random.Range(1, 7);

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
            
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                if (isOutside && !MysteryDice.useDiceOutside.Value)
                {
                    Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                    return;
                }
                ShowDefaultTooltip(randomEffect, diceRoll);

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
        }

        public void setupRustyRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad, EffectType.Mixed });
            RollToEffect.Add(3, new EffectType[] { });
            RollToEffect.Add(4, new EffectType[] { });
            RollToEffect.Add(5, new EffectType[] { });
            RollToEffect.Add(6, new EffectType[] { });
            doRustyRoll();
            
        }
        public void doRustyRoll()
        {
            try
            {
            int diceRoll = UnityEngine.Random.Range(1,7);
            int diceRoll2 = UnityEngine.Random.Range(1,15);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if(randomEffect==null) 
            {
                switch (diceRoll)
                {
                    case 3:
                        if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 3");
                        Networker.Instance.JackpotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(1, 2));
                        Misc.SafeTipMessage($"Rolled 3", "Spawning some scrap");
                        break;
                    case 4:
                        if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 4");
                        Networker.Instance.JackpotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(3, 4));
                        Misc.SafeTipMessage($"Rolled 4", "Spawning scrap");
                        break;
                    case 5:
                        if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 5");
                        Networker.Instance.JackpotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(5, 6));
                        Misc.SafeTipMessage($"Rolled 5", "Spawning more scrap");
                        break;
                    case 6:
                        RoundManager RM = RoundManager.Instance;
                        List<int> weightList = new List<int>(RM.currentLevel.spawnableScrap.Count);
                        for (int j = 0; j < RM.currentLevel.spawnableScrap.Count; j++)
                        {
                            weightList.Add(RM.currentLevel.spawnableScrap[j].rarity);
                        }
                        int[] weights = weightList.ToArray();
                        switch (diceRoll2)
                        {
                            case 1:
                                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 6 Eggs");
                                Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(4, 6), "Easter egg");
                                Misc.SafeTipMessage($"Hop Hop", "Explosive Eggs?");
                                break;
                            case 2:
                                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 6 Christmas");
                                Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(4, 7),"Gift");
                                Misc.SafeTipMessage($"HO HO HO", "Christmas Time!");
                                break;
                            case 3:
                                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 6 SSDD");
                                var item = RM.currentLevel.spawnableScrap[RM.GetRandomWeightedIndex(weights)].spawnableItem;
                                Networker.Instance.SameScrapServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(6, 8),item.itemName);
                                Misc.SafeTipMessage($"Wat?", "It's all the same?!?");
                                break;
                            default:
                                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Rusty 6 Normal");
                                Networker.Instance.JackpotServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, PlayerUser), UnityEngine.Random.Range(7, 8));
                                Misc.SafeTipMessage($"Rolled 6", "Spawning a lot of scrap!");
                                break;
                        }
                       
                        break;
                }
            }
            else
            {
                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
                
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                ShowDefaultTooltip(randomEffect, diceRoll);
            }

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
            
        }

        public void setupSaintRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { });
            RollToEffect.Add(2, new EffectType[] { EffectType.Good });
            RollToEffect.Add(3, new EffectType[] { EffectType.Good });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
            doSaintRoll();
        }
        public void doSaintRoll()
        {
            try
            {
                int diceRoll = UnityEngine.Random.Range(1,7);

                if (diceRoll == 1)
                {
                    Misc.SafeTipMessage($"Rolled 1", "Nothing happened");
                    return;
                }

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                PlaySoundBasedOnEffect(randomEffect.Outcome);

                if(diceRoll == 6)
                {
                    if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: Saint 6");
                    DebugMenuStuff.ShowSelectEffectMenu();
                    Misc.SafeTipMessage($"Rolled 6", "Choose an effect");
                    var who2 = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                    Networker.Instance.LogEffectsToOwnerServerRPC(who2, randomEffect.Name, diceRoll);
                    return;
                }

                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
            
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
        }

        
        public void setupSurfacedRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, [EffectType.Awful, EffectType.Bad,EffectType.Mixed]);
            RollToEffect.Add(2, [EffectType.Bad, EffectType.Awful,EffectType.Mixed]);
            RollToEffect.Add(3, [EffectType.Mixed, EffectType.Bad, EffectType.Good]);
            RollToEffect.Add(4, [EffectType.Good, EffectType.Mixed, EffectType.Bad]);
            RollToEffect.Add(5, [EffectType.Good, EffectType.Great,EffectType.Mixed]);
            RollToEffect.Add(6, [EffectType.Great, EffectType.Good,EffectType.Mixed]);
            doSurfacedRoll();
        }
        public void doSurfacedRoll()
        {
            try
            {
                var isOutside = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;

                var diceRoll = UnityEngine.Random.Range(1, 7);

                if (isOutside && !MysteryDice.useDiceOutside.Value) diceRoll = 1;

                var randomEffect = GetRandomEffect(diceRoll, MysteryDice.SurfacedPresent? SurfacedEffects : Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
            
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                if (isOutside && !MysteryDice.useDiceOutside.Value)
                {
                    Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
                    return;
                }
                ShowDefaultTooltip(randomEffect, diceRoll);
            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
            
        }

        public void setupSacrificerRoll()
        {
            RollToEffect.Clear();
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(3, new EffectType[] { EffectType.Awful, EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(5, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(6, new EffectType[] { EffectType.Mixed, EffectType.Bad });
            doSacrificerRoll();
        }
        public void doSacrificerRoll()
        {
            try
            {
                int diceRoll = UnityEngine.Random.Range(1,7);

                IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

                if (randomEffect == null) return;

                PlaySoundBasedOnEffect(randomEffect.Outcome);
                if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                randomEffect.Use();
           
                var who = wasCurse ? "A Cursed Player" : wasEnemy ? "An Enemy" : wasGhost ? "A ghost" : PlayerUser.playerUsername;
                Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);

                if (diceRoll == 1)
                {
                    Misc.SafeTipMessage($"Rolled 1...", "Run");
                    randomEffect = GetRandomEffect(diceRoll, Effects);
                    if(MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogDebug("Rolling Effect: "+ randomEffect.Name);
                    randomEffect.Use();
                
                    who = PlayerUser != null ? PlayerUser.playerUsername : "An Enemy";
                    Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
                }
                else
                    Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Outcome==EffectType.Bad ? "This could have been worse":"Awful");

            }
            catch (Exception e)
            {
                MysteryDice.CustomLogger.LogError("Roll error: "+ e);
            }
            new ReturnToShip().Use();
        }
        public void AprilFoolsRoll()
        {
            var diceRoll = Random.Range(0, 7);
            switch (diceRoll)
            {
                case 0:
                    setupChronosRoll();
                    break;
                case 1:
                    setupEmergencyRoll();
                    break;
                case 2:
                    setupSacrificerRoll();
                    break;
                case 3:
                    setupSaintRoll();
                    break;
                case 4:
                    setupGamblerRoll();
                    break;
                case 5:
                    setupSurfacedRoll();
                    break;
                case 6:
                    setupRustyRoll();
                    break;
                default:
                    RollToEffect.Clear();
                    SetupRollToEffectMapping();
                    Roll();
                    break;
            }
            RollToEffect.Clear();
            SetupRollToEffectMapping();
        }

        #endregion
    }
   

}
