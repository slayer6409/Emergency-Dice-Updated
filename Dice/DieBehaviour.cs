﻿using BepInEx.Configuration;
using BepInEx;
using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;

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
        public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();

        public PlayerControllerB PlayerUser = null;
        public enum ShowEffect
        {
            ALL,
            NONE,
            DEFAULT,
            RANDOM
        }
        public virtual void SetupDiceEffects()
        {
            foreach (IEffect effect in AllowedEffects)
                Effects.Add(effect);
        }
        public virtual void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
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

            if (DiceModel.GetComponent<Renderer>() != null)
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

                PlayerUser = playerHeldBy;

                ulong dropperID = playerHeldBy.playerClientId;
                GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GetItemFloorPosition(DiceModel.transform.parent.position), false);
                SyncDropServerRPC(dropperID, UnityEngine.Random.Range(0, 10));
            }
        }

        public virtual IEnumerator UseTimer(ulong userID, int spinTime)
        {
            if (!MysteryDice.randomSpinTime.Value) spinTime = 3;
            DiceModel.GetComponent<Spinner>().StartHyperSpinning(spinTime);

            yield return new WaitForSeconds(spinTime);

            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 0, 0, 0, null, false);
            DestroyObject();

            if (GameNetworkManager.Instance.localPlayerController.playerClientId == userID)
            {
                if (StartOfRound.Instance == null) yield break;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) yield break;

                if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion")
                {
                    Misc.SafeTipMessage($"Company penalty", "Do not try this again.");
                    (new Detonate()).Use();
                    yield break;
                }

                Roll();
            }
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
        }
        public virtual void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1, 7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();

            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            ShowDefaultTooltip(randomEffect, diceRoll);
        }
        public IEffect GetRandomEffect(int diceRoll, List<IEffect> effects)
        {
            List<IEffect> rolledEffects = new List<IEffect>();

            foreach (IEffect effect in Effects)
                if (RollToEffect[diceRoll].Contains(effect.Outcome))
                    rolledEffects.Add(effect);

            if (rolledEffects.Count == 0) return null;
            int randomEffectID = UnityEngine.Random.Range(0, rolledEffects.Count);
            return rolledEffects[randomEffectID];
        }

        public void PlaySoundBasedOnEffect(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Awful:
                    AudioSource.PlayClipAtPoint(MysteryDice.AwfulEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
                case EffectType.Bad:
                    AudioSource.PlayClipAtPoint(MysteryDice.BadEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
                default:
                    AudioSource.PlayClipAtPoint(MysteryDice.GoodEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
            }

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
                    effectTypeMessage = badMessages[UnityEngine.Random.Range(0, badMessages.Length)]; ;
                    break;
                case EffectType.Good:
                    effectTypeMessage = goodMessages[UnityEngine.Random.Range(0, goodMessages.Length)]; ;
                    break;
                case EffectType.Great:
                    effectTypeMessage = greatMessages[UnityEngine.Random.Range(0, greatMessages.Length)]; ;
                    break;
                case EffectType.Mixed:
                    effectTypeMessage = mixedMessages[UnityEngine.Random.Range(0, mixedMessages.Length)]; ;
                    break;
            }

            if (MysteryDice.DisplayResults.Value == ShowEffect.ALL)
            {
                normalMessage = true;
                message = effect.Tooltip;
            }
            else if (MysteryDice.DisplayResults.Value == ShowEffect.NONE)
            {
                message = effectTypeMessage;

            }
            else if (MysteryDice.DisplayResults.Value == ShowEffect.RANDOM)
            {
                int randint = UnityEngine.Random.Range(0, 101);
                if (randint <= 45)
                    message = effect.Tooltip;
                else
                    message = effectTypeMessage;
            }
            else if (MysteryDice.DisplayResults.Value == ShowEffect.DEFAULT)
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
            AllEffects.Add(new NeckBreak());
            AllEffects.Add(new SelectEffect());
            AllEffects.Add(new Fly());
            AllEffects.Add(new LeverShake());
            AllEffects.Add(new HyperShake());
            AllEffects.Add(new MovingLandmines());
            AllEffects.Add(new OutsideCoilhead());
            AllEffects.Add(new Arachnophobia());
            AllEffects.Add(new BrightFlashlight());
            AllEffects.Add(new IncreasedRate());
            AllEffects.Add(new DoorMalfunction());
            AllEffects.Add(new Purge());
            AllEffects.Add(new InfiniteStaminaAll());
            AllEffects.Add(new InfiniteStamina());
            AllEffects.Add(new Pathfinder());
            AllEffects.Add(new Shotgun());
            AllEffects.Add(new ShipTurret());
            AllEffects.Add(new TurretHell());
            AllEffects.Add(new SilentMine());
            AllEffects.Add(new ZombieToShip());
            AllEffects.Add(new InvertDoorLock());
            AllEffects.Add(new AlarmCurse());
            AllEffects.Add(new JumpscareGlitch());
            AllEffects.Add(new Armageddon());
            AllEffects.Add(new Beepocalypse());
            AllEffects.Add(new RebeliousCoilHeads());
            AllEffects.Add(new TurnOffLights());
            AllEffects.Add(new HealAndRestore());
            AllEffects.Add(new ScrapJackpot());
            AllEffects.Add(new Swap());
            AllEffects.Add(new ModifyPitch());
            AllEffects.Add(new MineOverflow());
            AllEffects.Add(new MineOverflowOutside());
            AllEffects.Add(new InstaJester());
            AllEffects.Add(new FakeFireExits());
            AllEffects.Add(new FireExitBlock());
            AllEffects.Add(new ReturnToShip());
            AllEffects.Add(new TeleportInside());
            AllEffects.Add(new BugPlague());
            AllEffects.Add(new ZombieApocalypse());
            AllEffects.Add(new Revive());
            AllEffects.Add(new Detonate());
            AllEffects.Add(new RandomStoreItem());
            AllEffects.Add(new RandomGreatStoreItem());
            AllEffects.Add(new BatteryDrain());
            AllEffects.Add(new EveryoneToSomeone());
            AllEffects.Add(new LightBurden());
            AllEffects.Add(new ItemDuplicator());
            AllEffects.Add(new HeavyBurden());
            CompleteEffects.Add(new DrunkForAll());
            AllEffects.Add(new Drunk());
            //AllEffects.Add(new ItemSwap()); //Need to be fixed
            AllEffects.Add(new GoldenTouch());
            AllEffects.Add(new Reroll());
            AllEffects.Add(new NeckSpin());
            AllEffects.Add(new GiveAllDice());
            AllEffects.Add(new InsideGiant());
            AllEffects.Add(new OutsideBugs());
            AllEffects.Add(new Maneaters());
            AllEffects.Add(new Delay());
            AllEffects.Add(new SpikeOverflowOutside());
            AllEffects.Add(new Lasso());
            AllEffects.Add(new Meteors());
            AllEffects.Add(new Barbers());
            AllEffects.Add(new SizeDifference());
            CompleteEffects.Add(new SizeDifferenceForAll());
            AllEffects.Add(new SizeDifferenceSwitcher());
            AllEffects.Add(new InvisibleEnemy());
            AllEffects.Add(new EmergencyMeeting());
            AllEffects.Add(new Ghosts());
            AllEffects.Add(new MerryChristmas());
            AllEffects.Add(new NutcrackerOutside());
            AllEffects.Add(new AllSameScrap());
            AllEffects.Add(new Eggs());
            AllEffects.Add(new EggFountain());
            AllEffects.Add(new FlashFountain());
            AllEffects.Add(new InsideDog());
            AllEffects.Add(new EggBoots());
            CompleteEffects.Add(new EggBootsForAll());
            AllEffects.Add(new TulipTrapeze());
            AllEffects.Add(new BlameGlitch());
            AllEffects.Add(new HappyDay());
            //AllEffects.Add(new AnythingGrenade());
            //AllEffects.Add(new TerminalLockout());
            if (MysteryDice.lethalThingsPresent)
            {
                AllEffects.Add(new TPTraps());
                AllEffects.Add(new MovingTPTraps());
                AllEffects.Add(new TpOverflowOutside());
                AllEffects.Add(new SilentTP());
                AllEffects.Add(new Friends());
                //AllEffects.Add(new SpeedyBoomba());
            }
            if (MysteryDice.LethalMonPresent) 
            {
                AllEffects.Add(new CatchEmAll());
                AllEffects.Add(new LegendaryCatch());
            }
            if (MysteryDice.LCOfficePresent)
            {
                AllEffects.Add(new InsideShrimps());
            }
            if (MysteryDice.SurfacedPresent)
            {
                AllEffects.Add(new MantisShrimps());
                AllEffects.Add(new SeaminesOutside());
                AllEffects.Add(new BerthaOutside());
                AllEffects.Add(new Bruce());
                AllEffects.Add(new Nemo());
                AllEffects.Add(new BellCrabs());
                AllEffects.Add(new UrchinIndoors());
                AllEffects.Add(new MineHardPlace());
                AllEffects.Add(new Flinger());
                AllEffects.Add(new BurgerFlippers());
            }
            if (MysteryDice.LCTarotCardPresent)
            {
                AllEffects.Add(new TarotCards());
            }
            if (MysteryDice.TakeyPlushPresent)
            {
                AllEffects.Add(new TakeySmol());
            }
            if (MysteryDice.CodeRebirthPresent) 
            {
                AllEffects.Add(new Tornado());
                AllEffects.Add(new CratesOutside());
                AllEffects.Add(new CratesInside());
            }

            List<IEffect> sortedList = AllEffects.OrderBy(o=>o.Name).ToList();
            CompleteEffects.AddRange(AllEffects);
            CompleteEffects = CompleteEffects.OrderBy(o => o.Name).ToList();
            AllEffects = sortedList;

            foreach (var effect in AllEffects)
            {
                ConfigEntry<bool> cfg;
                if (effect.Name == new SpikeOverflowOutside().Name || effect.Name == new BlameGlitch().Name) //Default off 
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Allowed Effects",
                    effect.Name,
                    false,
                    effect.Tooltip);
                }
                else  //Rest of them Default on
                {
                    cfg = MysteryDice.BepInExConfig.Bind<bool>("Allowed Effects",
                    effect.Name,
                    true,
                    effect.Tooltip);
                }
               
                effectConfigs.Add(cfg);
                if (cfg.Value)
                    AllowedEffects.Add(effect);
            }

            foreach (var effect in AllowedEffects)
            {
                switch (effect.Outcome)
                {
                    case EffectType.Awful:
                        AwfulEffects.Add(effect);
                        break;
                    case EffectType.Bad:
                        BadEffects.Add(effect);
                        break;
                    case EffectType.Mixed:
                        MixedEffects.Add(effect);
                        break;
                    case EffectType.Good:
                        GoodEffects.Add(effect);
                        break;
                    case EffectType.Great:
                        GreatEffects.Add(effect);
                        break;
                }
            }
        }
    }
   

}
