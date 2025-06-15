using MysteryDice.Patches;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class FreebirdEnemy : IEffect
    {
        public string Name => "Freebird Enemy";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "FREEBIRDDDDDDDDDDDDDDDDDDDDDD YEAH";
        
        public void Use()
        {
            Networker.Instance.SpawnFreebirdEnemyServerRPC();
        }

        public static void spawnEnemy()
        {
            var enemy = GetEnemies.allEnemies[UnityEngine.Random.Range(0, GetEnemies.allEnemies.Count)];
            var RM = RoundManager.Instance;
            
            Vector3 position = Vector3.zero;
            
            EnemyVent randomVent = RM.allEnemyVents[UnityEngine.Random.Range(0, RM.allEnemyVents.Length)];
            
            if (enemy.isOutsideEnemy || enemy.isDaytimeEnemy)
            {
                position = RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            }
            else
            {
                position = randomVent.floorNode.position;
            }
            GameObject gameObject = UnityEngine.Object.Instantiate(
                enemy.enemyPrefab,
                position,
                Quaternion.identity);
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
            var netobj = gameObject.GetComponent<NetworkObject>();
            netobj.Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            Networker.Instance.FreebirdEnemyServerRPC(netobj.NetworkObjectId);
        }
        public static void spawnEnemy(string name, Vector3 pos)
        {
            var enemy = GetEnemies.allEnemies.Find(x => x.enemyName == name);
            GameObject gameObject = UnityEngine.Object.Instantiate(
                enemy.enemyPrefab,
                pos,
                Quaternion.identity);
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, UnityEngine.Random.Range(0, 360), gameObject.transform.eulerAngles.z);
            var netobj = gameObject.GetComponent<NetworkObject>();
            netobj.Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            Networker.Instance.FreebirdEnemyServerRPC(netobj.NetworkObjectId);
        }

        public static void fixEnemy(ulong id)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                obj.gameObject.AddComponent<freebirdMaker>();
                
            }
        }
        
        public static void fixTrap(ulong id)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                obj.gameObject.AddComponent<freebirdTrapMaker>();
                if (MysteryDice.CodeRebirthPresent)
                {
                    Networker.Instance.StartCoroutine(MovingBeartraps.fixSpiney(obj));
                }
            }
        }
    }
    
    public class freebirdMaker : MonoBehaviour
    {
        public NavMeshAgent agent;
        private AudioSource audiosrc;
        private EnemyAI _enemyAI;
        public bool isEvil = false;

        public void Start()
        {
            if (agent == null) agent = gameObject.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            if (_enemyAI == null) _enemyAI = gameObject.GetComponent<EnemyAI>();
            audiosrc = gameObject.AddComponent<AudioSource>();
            Networker.Instance.FreebirdAudioSources.Add(audiosrc);
            audiosrc.loop = true;
            audiosrc.playOnAwake = true;
            audiosrc.maxDistance = 30;
            audiosrc.minDistance = 0;
            audiosrc.volume = MysteryDice.SoundVolume.Value;
            audiosrc.rolloffMode = AudioRolloffMode.Custom;
            audiosrc.spatialBlend = 1;
            audiosrc.dopplerLevel = 0;
            if (MysteryDice.CopyrightFree.Value)
            {
                audiosrc.volume -= 0.08f;
                audiosrc.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("SpazzmaticaPolka");
            }
            else
            {
                audiosrc.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("Freebird");
            }

            audiosrc.Play();
        }

        public void OnDestroy()
        {
            Networker.Instance.FreebirdAudioSources.Remove(audiosrc);
        }

        public void Update()
        {
            if (_enemyAI.isEnemyDead)
            {
                audiosrc.Stop();
                Destroy(this);
            }
            if (agent==null) return;
            agent.acceleration = 999;
            agent.angularSpeed = 360;
            agent.speed = 20;
            if (isEvil)
            {
                var target = Misc.GetRandomAlivePlayer();
                if (target != null && agent.isOnNavMesh)
                {
                    Debug.Log($"Setting destination to {target.name} at {target.transform.position}");
                    agent.destination = target.transform.position;
                }
                else
                {
                    Debug.LogWarning("No valid target or agent not on NavMesh.");
                }
            }
        }
    }
    public class freebirdTrapMaker : MonoBehaviour
    {
        public NavMeshAgent agent;
        private AudioSource audiosrc;
        public bool isEvil = false;
        public void Start()
        {
            if (agent == null) agent = gameObject.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            audiosrc = gameObject.AddComponent<AudioSource>();
            audiosrc.loop = true;
            audiosrc.maxDistance = 30;
            audiosrc.minDistance = 0;
            audiosrc.volume = 0.8f;
            audiosrc.rolloffMode = AudioRolloffMode.Custom;
            audiosrc.spatialBlend = 1;
            audiosrc.dopplerLevel = 0;
            if (MysteryDice.CopyrightFree.Value)
            {
                audiosrc.volume -= 0.08f;
                audiosrc.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("SpazzmaticaPolka");
            }
            else
            {
                audiosrc.clip = MysteryDice.LoadedAssets2.LoadAsset<AudioClip>("Freebird");
            }
            audiosrc.Play();
        }

        public void Update()
        {
            if (agent==null) return;
            agent.acceleration = 999;
            agent.angularSpeed = 360;
            agent.speed = 20;
            if (isEvil)
            {
                agent.destination = Misc.GetRandomAlivePlayer().transform.position;
            }
            
        }
    }
}
