using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using System.Reflection;
using CodeRebirth.src.Content.Maps;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace MysteryDice.Effects
{
    internal class MovingCrates : IEffect
    {
        public string Name => "Moving Crates";

        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "They ship themselves";

        public void Use()
        {
            Networker.Instance.MovingCratesServerRPC();
        }

        public static IEnumerator MakeMovingCrates()
        {
            var crates = Misc.getAllTraps().Where(x=>x.name.Contains("Crate")||x.name.Contains("Safe"));
            var RM = RoundManager.Instance;
            var crateAmount = Random.Range(2, 6);
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < crateAmount; i++)
            {
                var trapToSpawn = crates.ToList().OrderBy(_ => Random.value).First();
                var position = Random.value>0.5 ? RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position : RM.insideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
                var agent = GameObject.Instantiate(MysteryDice.AgentObjectPrefab, position, Quaternion.Euler(0, 0, 0));
                var trap = GameObject.Instantiate(trapToSpawn.prefab, position, Quaternion.Euler(0, 0, 0));
                var netobj = trap.GetComponent<NetworkObject>();
                netobj.Spawn(destroyWithScene:true);
                agent.GetComponent<NetworkObject>().Spawn(destroyWithScene:true);
                var smartAgentNavigator = agent.GetComponent<SmartAgentNavigator>();
                var mm = trap.AddComponent<MakeMove>();
                trap.transform.SetParent(agent.transform);
                trap.transform.localPosition = Vector3.zero;
                trap.transform.localRotation = Quaternion.identity;
                mm.Initialize(smartAgentNavigator);
                mm.enabled = true;
                SceneManager.MoveGameObjectToScene(agent, RoundManager.Instance.mapPropsContainer.scene);
                if (trapToSpawn.name.Contains("Mimic"))
                {
                    ItemCrate ic = trap.GetComponent<ItemCrate>();
                    ic.health=1;
                    ic.digProgress=2;
                    if (ic.pickable != null) ic.pickable.IsLocked = false;
                    Networker.Instance.CrateOpenServerRPC(netobj.NetworkObjectId);
                }
            }
          
        }

        public static IEnumerator crateOpener(ulong crateId)
        {
            yield return new WaitForSeconds(2f);
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(crateId, out var networkObj))
            {
                GameObject trap = networkObj.gameObject; 
                ItemCrate ic = trap.GetComponent<ItemCrate>();
                ic.OpenCrate();
            }
               
            
        }
        
     }
}
