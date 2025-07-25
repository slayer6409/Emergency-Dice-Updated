using MysteryDice.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiversityDungeonGen;
using GameNetcodeStuff;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Bald : IEffect
    {
        public string Name => "Chrome Dome";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Blindingly Bald";
        public void Use()
        {
            if (StartOfRound.Instance is null) return;
            if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;
            var glitch = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(x => x.playerSteamId == 76561198984467725);
            if (glitch != null)
            {
                if (!glitch.isPlayerDead) Networker.Instance.BaldServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, glitch));
                else Networker.Instance.BaldServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, Misc.GetRandomAlivePlayer()));
            }
            else
            {
                Networker.Instance.BaldServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, Misc.GetRandomAlivePlayer()));
            }
        }

        public static void SpawnBald(int playerID)
        {
            var player = Misc.GetPlayerByUserID(playerID);
            GameObject gameObject = UnityEngine.Object.Instantiate(
                GetEnemies.FlashTurret.prefabToSpawn,
                player.transform.position,
                Quaternion.identity);
            var netobj = gameObject.GetComponent<NetworkObject>();
            netobj.Spawn(destroyWithScene: true);
            SceneManager.MoveGameObjectToScene(gameObject, RoundManager.Instance.mapPropsContainer.scene);
            Networker.Instance.FixBaldClientRpc(playerID, netobj.NetworkObjectId);
        }

        public static void FixBald(int playerID, GameObject bald)
        {
            var bh = bald.AddComponent<baldHandler>();
            bh.player = Misc.GetPlayerByUserID(playerID);
            bh.toLink = bald;
            var snp = bald.GetComponentInChildren<ScanNodeProperties>();
            snp.headerText = "Bald";
            snp.subText = "You're Bald"; 
            bald.GetComponent<CapsuleCollider>().isTrigger = true;
            bald.transform.Find("FlashTurret").gameObject.SetActive(false);
        }
    }

    public class baldHandler : MonoBehaviour
    {
        public PlayerControllerB player;
        public GameObject toLink;

        public void Start()
        {
            Instantiate(MysteryDice.BaldPrefab, toLink.transform.position, Quaternion.identity, toLink.transform);
        }
        public void Update()
        {
            toLink.transform.position = player.transform.position+new Vector3(0f, .55f, 0f);
        }
    }
}
