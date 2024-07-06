using BepInEx.Configuration;
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
        public static List<IEffect> AllowedEffects = new List<IEffect>();

        public static bool LogEffectsToConsole = false;

        protected GameObject DiceModel;
        public List<IEffect> Effects = new List<IEffect>();
        public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();

        public static bool randomUseTimer = false;

        public PlayerControllerB PlayerUser = null;

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
           
            DiceModel.SetActive(false);
            base.PocketItem();

        }
        public override void EquipItem()
        {
            DiceModel.SetActive(true);
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
                SyncDropServerRPC(dropperID);
            }
        }

        public virtual IEnumerator UseTimer(ulong userID)
        {
            float spinSeconds = 3f;
            if (randomUseTimer)
            {
                spinSeconds = (float)UnityEngine.Random.Range(0, 10);
            }

            DiceModel.GetComponent<Spinner>().StartHyperSpinning(spinSeconds);

            yield return new WaitForSeconds(spinSeconds);

            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 0, 0,0,null,false);
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
        public virtual void SyncDropServerRPC(ulong userID)
        {
            if (!IsHost)
                DropAndBlock(userID);

            SyncDropClientRPC(userID);
        }
        [ClientRpc]
        public virtual void SyncDropClientRPC(ulong userID)
        {
            DropAndBlock(userID);
        }

        public virtual void DropAndBlock(ulong userID)
        {
            grabbable = false;
            grabbableToEnemies = false;
            DiceModel.SetActive(true);
            StartCoroutine(UseTimer(userID));
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


            if (randomEffect.ShowDefaultTooltip)
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            else
                Misc.SafeTipMessage($"Rolled {diceRoll}", randomEffect.Tooltip);
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
        public static void ShowDefaultTooltip(EffectType effectType, int diceRoll)
        {
            switch (effectType)
            {
                case EffectType.Awful:
                    Misc.SafeTipMessage($"Rolled {diceRoll}", ":)");
                    break;
                case EffectType.Bad:
                    Misc.SafeTipMessage($"Rolled {diceRoll}", "Uh oh");
                    break;
                case EffectType.Good:
                    Misc.SafeTipMessage($"Rolled {diceRoll}", "Enjoy.");
                    break;
                case EffectType.Great:
                    Misc.SafeTipMessage($"Rolled {diceRoll}", "Lucky.");
                    break;
                case EffectType.Mixed:
                    Misc.SafeTipMessage($"Rolled {diceRoll}", "Debatable");
                    break;
            }
        }

        public static void Config()
        {
            ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "Emergency Dice.cfg"), true);

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

            foreach(var effect in AllEffects)
            {
                ConfigEntry<bool> cfg = MysteryDice.BepInExConfig.Bind<bool>("Allowed Effects",
                    effect.Name,
                    true,
                    effect.Tooltip);

                if (cfg.Value)
                    AllowedEffects.Add(effect);
            }
        }
    }


}
