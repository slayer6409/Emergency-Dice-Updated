using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class BadLovers : IEffect
    {
        public string Name => "Bad Romance";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "<3";

        public void Use()
        {
            if (!Misc.canDiceYet())
            {
                Networker.Instance.QueueSpecificDiceEffectServerRPC(StartOfRound.Instance.localPlayerController.playerUsername,"Bad Lovers");
                return;
            }
            var playerScripts = new List<PlayerControllerB>(StartOfRound.Instance.allPlayerScripts);
            playerScripts = playerScripts.Where(x => !x.isPlayerDead && x.isActiveAndEnabled && x.isPlayerControlled).OrderBy(x => Random.value).ToList();

            ulong p1 = playerScripts[0].actualClientId;
            var enemiesAlive = RoundManager.Instance.SpawnedEnemies.Where(x => !x.isEnemyDead).ToList();
            if (enemiesAlive.Any())
            {
                int randomIndex = Random.Range(0, enemiesAlive.Count);
                var enemy = enemiesAlive[randomIndex];
                var p2 = enemy.GetComponent<NetworkObject>().NetworkObjectId;
                Networker.Instance.makeBadLoverServerRPC(p1, p2);
            }
            
        } 
        public static void makeLovers(ulong p1, ulong enemyAI)
        {
            var localPlayer = StartOfRound.Instance.localPlayerController;
            if (localPlayer.actualClientId == p1)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyAI, out var networkObj))
                {
                    GameObject obj = networkObj.gameObject;
                    var eai = obj.GetComponent<EnemyAI>();
                    badLoverScriptb bs = eai.gameObject.AddComponent<badLoverScriptb>();
                    badLoverScripta ls = localPlayer.gameObject.AddComponent<badLoverScripta>();
                    ls.me = localPlayer;
                    ls.lover = eai;
                    bs.me = eai;
                    bs.lover = localPlayer;
                    Misc.SafeTipMessage("Lovers!",$"You are now in love with a random {ls.lover.enemyType}");
                }
            }
        }
    } 
    public class badLoverScripta : MonoBehaviour
    {
        public PlayerControllerB me;
        public EnemyAI lover;
        public void Update()
        {
            if (lover.isEnemyDead || (lover ==null && (!StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.inShipPhase)))
            {
                me.KillPlayer(Vector3.up, true, CauseOfDeath.Abandoned,0);
                Destroy(this);
            }
            if(me.isPlayerDead)Destroy(this);
        }
        public void removeLover()
        {
            Destroy(this);
        }
    }
    public class badLoverScriptb : MonoBehaviour
    {
        public EnemyAI me;
        public PlayerControllerB lover;
        public void Update()
        {
            if (lover.isPlayerDead)
            {
                me.KillEnemy(true);
                if (!me.isEnemyDead)
                {
                    Networker.Instance.despawnEnemyServerRpc(me.NetworkObject.NetworkObjectId);
                }
                Destroy(this);
            }
            if(me.isEnemyDead)Destroy(this);
        }

        public void removeLover()
        {
            Destroy(this);
        }
    }
}
