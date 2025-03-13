using System;
using BepInEx.Configuration;
using MysteryDice.Effects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using MysteryDice.Patches;
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
        protected GameObject DiceModel;
        public List<IEffect> Effects = new List<IEffect>();
        private static List<string> surfacedNames = new List<string>();
        public List<IEffect> SurfacedEffects = new List<IEffect>();
        public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();
        public bool wasEnemy = false;

        public PlayerControllerB PlayerUser = null;
        public EnemyAI EnemyUser = null;
        public enum ShowEffect
        {
            ALL,
            NONE,
            DEFAULT,
            RANDOM
        }
        public virtual void SetupDiceEffects()
        {
            Effects = new List<IEffect>(AllowedEffects);
            SurfacedEffects = AllowedEffects.Where(e=>surfacedNames.Contains(e.Name)).ToList();
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
            DiceModel = gameObject.transform.Find("Model").gameObject;
            DiceModel.AddComponent<Spinner>();
            SetupDiceEffects();
            SetupRollToEffectMapping();
        }
        
        public override void PocketItem()
        {
            if (DiceModel.GetComponent<Renderer>())
            {
                DiceModel.GetComponent<Renderer>().enabled = true;
            }
            DiceModel.SetActive(false);
            base.PocketItem();

        }
        public override void EquipItem()
        {
            DiceModel.SetActive(true);
            if (DiceModel.GetComponent<Renderer>() != null)
            {
                DiceModel.GetComponent<Renderer>().enabled = true;
            }
            base.EquipItem();
            PlayerUser = this.playerHeldBy;
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            DiceModel.SetActive(true);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                if (StartOfRound.Instance == null) return;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
                if(playerHeldBy == null) return;
                PlayerUser = playerHeldBy;

                ulong dropperID = playerHeldBy.actualClientId;
                GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GetItemFloorPosition(DiceModel.transform.parent.position), false);
                SyncDropServerRPC(dropperID, UnityEngine.Random.Range(0, 10));
            }
        }

        public virtual IEnumerator UseTimer(ulong userID, int spinTime)
        {
            if (!MysteryDice.randomSpinTime.Value) spinTime = 3;
            DiceModel.GetComponent<Spinner>().StartHyperSpinning(spinTime);

            yield return new WaitForSeconds(spinTime);

            if(MysteryDice.doDiceExplosion.Value) Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 0, 0, 0, null, false);
            try
            {
                if (userID == 6409046)
                {
                    wasEnemy = true;
                    Roll();
                }
                else
                {
                    if (GameNetworkManager.Instance.localPlayerController.actualClientId == userID)
                    {
                        if (StartOfRound.Instance is null) yield break;
                        if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) yield break;

                        if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion" || StartOfRound.Instance.currentLevel.PlanetName == "98 Galetry" )
                        {
                            Misc.SafeTipMessage($"Company penalty", "Do not try this again.");
                            doPenalty();
                            yield break;
                        }
                        Roll();
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
        public virtual void SyncDropServerRPC(ulong userID, int Timer)
        {
            if (!IsHost) DropAndBlock(userID, Timer);

            SyncDropClientRPC(userID, Timer);
        }
        [ClientRpc]
        public virtual void SyncDropClientRPC(ulong userID, int Timer)
        {
            DropAndBlock(userID, Timer);
        } 

        public virtual void DropAndBlock(ulong userID, int Timer)
        {
            if(userID==6409046) wasEnemy = true;
            grabbable = false;
            grabbableToEnemies = false;
            DiceModel.SetActive(true);
            StartCoroutine(UseTimer(userID, Timer));
        } 
        public virtual void DestroyObject()
        {
            grabbable = false;
            grabbableToEnemies = false;
            deactivated = true;
            if (radarIcon != null)
            {
                GameObject.Destroy(radarIcon.gameObject);
            }
            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                GameObject.Destroy(componentsInChildren[i]);
            }
            Collider[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Collider>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                GameObject.Destroy(componentsInChildren2[j]);
            }
            if(IsHost) NetworkObject.Despawn();
        }
        public virtual void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1, 7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();

            
            var who = !wasEnemy ? PlayerUser.playerUsername : "An Enemy";
            Networker.Instance.LogEffectsToOwnerServerRPC(who, randomEffect.Name, diceRoll);
            ShowDefaultTooltip(randomEffect, diceRoll);
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

        public static void doPenalty()
        {
            if (MysteryDice.NavMeshInCompanyPresent)
            {
                Networker.Instance.doPenaltyServerRPC(10);
            }
            else
            {
                (new Detonate()).Use();
            }
            Misc.SafeTipMessage($"Penalty", "Next time roll it inside :)");
        }

        public static void Config()
        {
            MysteryDice.MainRegisterNewEffect(new NeckBreak());
            MysteryDice.MainRegisterNewEffect(new SelectEffect());
            MysteryDice.MainRegisterNewEffect(new Fly());
            MysteryDice.MainRegisterNewEffect(new LeverShake());
            MysteryDice.MainRegisterNewEffect(new HyperShake());
            MysteryDice.MainRegisterNewEffect(new MovingLandmines());
            MysteryDice.MainRegisterNewEffect(new OutsideCoilhead());
            MysteryDice.MainRegisterNewEffect(new Arachnophobia());
            MysteryDice.MainRegisterNewEffect(new BrightFlashlight());
            MysteryDice.MainRegisterNewEffect(new IncreasedRate());
            MysteryDice.MainRegisterNewEffect(new DoorMalfunction());
            MysteryDice.MainRegisterNewEffect(new Purge());
            MysteryDice.MainRegisterNewEffect(new InfiniteStaminaAll());
            MysteryDice.MainRegisterNewEffect(new InfiniteStamina());
            MysteryDice.MainRegisterNewEffect(new Pathfinder());
            MysteryDice.MainRegisterNewEffect(new Shotgun());
            MysteryDice.MainRegisterNewEffect(new ShipTurret());
            MysteryDice.MainRegisterNewEffect(new TurretHell());
            MysteryDice.MainRegisterNewEffect(new SilentMine());
            MysteryDice.MainRegisterNewEffect(new ZombieToShip());
            MysteryDice.MainRegisterNewEffect(new InvertDoorLock());
            MysteryDice.MainRegisterNewEffect(new AlarmCurse());
            MysteryDice.MainRegisterNewEffect(new JumpscareGlitch());
            MysteryDice.MainRegisterNewEffect(new Armageddon());
            MysteryDice.MainRegisterNewEffect(new Beepocalypse());
            MysteryDice.MainRegisterNewEffect(new RebeliousCoilHeads());
            MysteryDice.MainRegisterNewEffect(new TurnOffLights());
            MysteryDice.MainRegisterNewEffect(new HealAndRestore());
            MysteryDice.MainRegisterNewEffect(new ScrapJackpot());
            MysteryDice.MainRegisterNewEffect(new Swap());
            MysteryDice.MainRegisterNewEffect(new ModifyPitch());
            MysteryDice.MainRegisterNewEffect(new MineOverflow());
            MysteryDice.MainRegisterNewEffect(new MineOverflowOutside());
            MysteryDice.MainRegisterNewEffect(new InstaJester());
            MysteryDice.MainRegisterNewEffect(new FakeFireExits());
            MysteryDice.MainRegisterNewEffect(new FireExitBlock());
            MysteryDice.MainRegisterNewEffect(new ReturnToShip());
            MysteryDice.MainRegisterNewEffect(new ReturnToShipTogether());
            MysteryDice.MainRegisterNewEffect(new TeleportInside());
            MysteryDice.MainRegisterNewEffect(new BugPlague());
            MysteryDice.MainRegisterNewEffect(new ZombieApocalypse());
            MysteryDice.MainRegisterNewEffect(new Revive());
            MysteryDice.MainRegisterNewEffect(new Detonate());
            MysteryDice.MainRegisterNewEffect(new RandomStoreItem());
            MysteryDice.MainRegisterNewEffect(new RandomGreatStoreItem());
            MysteryDice.MainRegisterNewEffect(new BatteryDrain());
            MysteryDice.MainRegisterNewEffect(new EveryoneToSomeone());
            MysteryDice.MainRegisterNewEffect(new LightBurden());
            MysteryDice.MainRegisterNewEffect(new ItemDuplicator());
            MysteryDice.MainRegisterNewEffect(new HeavyBurden());
            MysteryDice.MainRegisterNewEffect(new DrunkForAll(),false,true);
            MysteryDice.MainRegisterNewEffect(new Drunk());
            MysteryDice.MainRegisterNewEffect(new GoldenTouch());
            MysteryDice.MainRegisterNewEffect(new Reroll());
            MysteryDice.MainRegisterNewEffect(new NeckSpin());
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
            MysteryDice.MainRegisterNewEffect(new EmergencyMeeting());
            MysteryDice.MainRegisterNewEffect(new Ghosts());
            MysteryDice.MainRegisterNewEffect(new MerryChristmas());
            MysteryDice.MainRegisterNewEffect(new NutcrackerOutside());
            MysteryDice.MainRegisterNewEffect(new AllSameScrap());
            MysteryDice.MainRegisterNewEffect(new Eggs());
            MysteryDice.MainRegisterNewEffect(new EggFountain());
            MysteryDice.MainRegisterNewEffect(new FlashFountain());
            MysteryDice.MainRegisterNewEffect(new InsideDog());
            MysteryDice.MainRegisterNewEffect(new EggBoots());
            MysteryDice.MainRegisterNewEffect(new EggBootsForAll(),false,true);
            MysteryDice.MainRegisterNewEffect(new RerollALL(),false,true);
            MysteryDice.MainRegisterNewEffect(new TulipTrapeze());
            MysteryDice.MainRegisterNewEffect(new BlameGlitch(),true);
            MysteryDice.MainRegisterNewEffect(new HappyDay());
            MysteryDice.MainRegisterNewEffect(new SpicyNuggies());
            MysteryDice.MainRegisterNewEffect(new Martyrdom());
            MysteryDice.MainRegisterNewEffect(new Lovers());
            MysteryDice.MainRegisterNewEffect(new BIGSpike(), true);
            MysteryDice.MainRegisterNewEffect(new SpeedUp(),true, true);
            MysteryDice.MainRegisterNewEffect(new GiveCredits());
            MysteryDice.MainRegisterNewEffect(new RemoveCredits());
            MysteryDice.MainRegisterNewEffect(new BigDelivery());
            MysteryDice.MainRegisterNewEffect(new DorjesDream());
            MysteryDice.MainRegisterNewEffect(new MoreLives());
            MysteryDice.MainRegisterNewEffect(new LizzieDog());
            MysteryDice.MainRegisterNewEffect(new WhereDidMyFriendsGo());
            MysteryDice.MainRegisterNewEffect(new Confusion());
            MysteryDice.MainRegisterNewEffect(new StupidConfusion(), true);
            MysteryDice.MainRegisterNewEffect(new NutsBaby());
            MysteryDice.MainRegisterNewEffect(new Papercut(), true);
            MysteryDice.MainRegisterNewEffect(new BadLovers());
            MysteryDice.MainRegisterNewEffect(new OopsAllBlank());
            MysteryDice.MainRegisterNewEffect(new AllAtOnce(), true, true);
            MysteryDice.MainRegisterNewEffect(new Unkillable());
            MysteryDice.MainRegisterNewEffect(new FreebirdEnemy(), true);
            //MysteryDice.MainRegisterNewEffect(new TargetPractice(), true); //it bad no do this
            
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
                MysteryDice.MainRegisterNewEffect(new BIGBertha());
                surfacedNames.Add(new MovingSeaTraps().Name);
                MysteryDice.MainRegisterNewEffect(new MovingSeaTraps());
                surfacedNames.Add(new BigFling().Name);
                MysteryDice.MainRegisterNewEffect(new BigFling());
                surfacedNames.Add(new SuperFlinger().Name);
                MysteryDice.MainRegisterNewEffect(new SuperFlinger());
                surfacedNames.Add(new Horseshootnt().Name);
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
                MysteryDice.MainRegisterNewEffect(new TakeySmol());
            }
            
            if (MysteryDice.CodeRebirthPresent) 
            {
                if(CodeRebirthCheckConfigs.checkTornadoConfig()) MysteryDice.MainRegisterNewEffect(new Tornado());
                if(CodeRebirthCheckConfigs.checkCrateConfig()) MysteryDice.MainRegisterNewEffect(new CratesOutside());
                if(CodeRebirthCheckConfigs.checkCrateConfig()) MysteryDice.MainRegisterNewEffect(new CratesInside());
                if(CodeRebirthCheckConfigs.checkFanConfig()) MysteryDice.MainRegisterNewEffect(new Fans());
                if(CodeRebirthCheckConfigs.checkFlashConfig()) MysteryDice.MainRegisterNewEffect(new Flashers());
                if(CodeRebirthCheckConfigs.checkMicrowaveConfig()) MysteryDice.MainRegisterNewEffect(new Microwave());
                if(CodeRebirthCheckConfigs.checkFanConfig()) MysteryDice.MainRegisterNewEffect(new MovingFans());
                if(CodeRebirthCheckConfigs.checkFanConfig()) MysteryDice.MainRegisterNewEffect(new SkyFan(),true); //Need to turn shadows off
                if(CodeRebirthCheckConfigs.checkFanConfig() && CodeRebirthCheckConfigs.checkFlashConfig()) MysteryDice.MainRegisterNewEffect(new Paparazzi());
                if(CodeRebirthCheckConfigs.checkBearTrapConfig()) MysteryDice.MainRegisterNewEffect(new MovingBeartraps());
                MysteryDice.MainRegisterNewEffect(new TheRumbling());
                if(CodeRebirthCheckConfigs.checkFanConfig()) MysteryDice.MainRegisterNewEffect(new FollowerFan());
                if(CodeRebirthCheckConfigs.checkJanitorConfig()&&CodeRebirthCheckConfigs.checkTransporterConfig()) MysteryDice.MainRegisterNewEffect(new CleaningCrew());
                if(CodeRebirthCheckConfigs.checkTransporterConfig()) MysteryDice.MainRegisterNewEffect(new FreebirdJimothy());
                if(CodeRebirthCheckConfigs.checkFlashConfig()) MysteryDice.MainRegisterNewEffect(new Bald());
                if(CodeRebirthCheckConfigs.checkZortConfig()) MysteryDice.MainRegisterNewEffect(new Zortin());
                if(CodeRebirthCheckConfigs.checkZortConfig()) MysteryDice.MainRegisterNewEffect(new ZortinTwo());
            }
            // if (!MysteryDice.DisableSizeBased.Value)
            // {
            //     MysteryDice.MainRegisterNewEffect(new SizeDifference());
            //     MysteryDice.MainRegisterNewEffect(new SizeDifferenceForAll(),false,true);
            //     MysteryDice.MainRegisterNewEffect(new SizeDifferenceSwitcher());
            // }
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

            List<IEffect> sortedList = AllEffects.OrderBy(o => o.Name).ToList();
            CompleteEffects.AddRange(AllEffects);
            CompleteEffects = CompleteEffects.OrderBy(o => o.Name).ToList();
            AllEffects = sortedList;
        }
    }
   

}
