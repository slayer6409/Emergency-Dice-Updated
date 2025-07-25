using System;
using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using GameNetcodeStuff;
using JetBrains.Annotations;
using MysteryDice.MiscStuff;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class MovingFans : IEffect
    {
        public string Name => "Moving Fans";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Fan-tastic";

        public void Use()
        {
            Networker.Instance.MovingFansServerRPC();
        }
        public static IEnumerator WaitForTrapInit(string trapName, bool follower = false, int followerID=0)
        {
            yield return new WaitForSeconds(5f);
            if (!RoundManager.Instance.IsHost) yield break;
            
            foreach (var trap in GameObject.FindObjectsOfType<GameObject>().Where(c => c.name.ToUpper().Contains(trapName.ToUpper())))
            {
                var no = trap.GetComponent<NetworkObject>();
                if (no == null) continue;
                
                if(no.gameObject.name.ToUpper().Contains("GAL")||no.gameObject.name.ToUpper().Contains("CHARGER")) continue;
                if (!trap)
                {
                    MysteryDice.CustomLogger.LogWarning("Trap is null.");
                    continue;
                } 
                var trapParent = trap.transform.parent;
                if (trapParent != null)
                {
                    if (trapName.ToUpper() == "SEAMINE" && trapParent.name.ToUpper().Contains("BERTHA")) continue;
                    if (trapParent.name.ToUpper().Contains(trapName.ToUpper())) continue;
                }
                var targetObject = trap.gameObject;
                if (targetObject.GetComponent<MakeMove>() == null)
                {
                    var agent = GameObject.Instantiate(MysteryDice.AgentObjectPrefab, targetObject.transform.position, Quaternion.Euler(0, 0, 0));
                    //if(trapName=="Bertha") agent.transform.localScale = new Vector3(0.19f, 0.19f, 0.19f);
                    if(trapName=="SeaMine") agent.transform.localScale = new Vector3(0.4573873f, 0.4573873f, 0.4573873f);
                    var smartAgentNavigator = agent.GetComponent<SmartAgentNavigator>();
                    var mm = targetObject.AddComponent<MakeMove>();
                    if (follower) mm.follows = Misc.GetPlayerByUserID(followerID);
                    agent.GetComponent<NetworkObject>().Spawn(destroyWithScene:true);
                    trap.transform.SetParent(agent.transform);
                    targetObject.transform.localPosition = Vector3.zero;
                    trap.transform.localPosition = new Vector3(0, 0.2f, 0);
                    targetObject.transform.localRotation = Quaternion.identity;
                    mm.Initialize(smartAgentNavigator);
                    mm.enabled = true;
                    SceneManager.MoveGameObjectToScene(agent, RoundManager.Instance.mapPropsContainer.scene);
                }
            }
        }

        public static IEnumerator freebirdTrapSpawn(string trapName, bool random=false)
        {
            var allTraps = Misc.getAllTraps()
                .GroupBy(x => x.name)
                .Select(g => g.First())
                .ToList();
            trap trapToSpawn;
            if (random)
            {
                trapToSpawn = allTraps.OrderBy(_=> Random.value).First();
            }
            else
            {
                trapToSpawn = allTraps.Find(x => x.name == trapName);
            }
            
            var RM = RoundManager.Instance;
            yield return new WaitForSeconds(2f);
            var position = Random.value>0.5 ? RM.outsideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position : RM.insideAINodes[UnityEngine.Random.Range(0, RM.outsideAINodes.Length)].transform.position;
            var agent = GameObject.Instantiate(MysteryDice.AgentObjectPrefab, position, Quaternion.Euler(0, 0, 0));
            var trap = GameObject.Instantiate(trapToSpawn.prefab, position, Quaternion.Euler(0, 0, 0));
            trap.GetComponent<NetworkObject>().Spawn(destroyWithScene:true);
            agent.GetComponent<NetworkObject>().Spawn(destroyWithScene:true);
            var smartAgentNavigator = agent.GetComponent<SmartAgentNavigator>();
            var mm = trap.AddComponent<MakeMove>();
            trap.transform.SetParent(agent.transform);
            trap.transform.localPosition = new Vector3(0, 0.2f, 0);
            trap.transform.localRotation = Quaternion.identity;
            mm.Initialize(smartAgentNavigator);
            mm.enabled = true;
            SceneManager.MoveGameObjectToScene(agent, RoundManager.Instance.mapPropsContainer.scene);
            if(StartOfRound.Instance.IsHost)
                Networker.Instance.FreebirdTrapClientRPC(agent.GetComponent<NetworkObject>().NetworkObjectId);
        } 
    }

    
    public class MakeMove : MonoBehaviour
    { 
        private float _pathTimer = 0f;
        

        private GameObject[] outsidePoints;
        private GameObject[] InsidePoints;
        public SmartAgentNavigator agent;
        public bool currently = false;
        [CanBeNull] public PlayerControllerB follows = null;
        private Vector3 _currentTargetPosition = Vector3.zero;
        bool _currentIsInside = false;
        private void Awake()
        {
            
            outsidePoints = RoundManager.Instance.outsideAINodes;
            InsidePoints = RoundManager.Instance.insideAINodes;

            if (outsidePoints.Length == 0 || InsidePoints.Length == 0)
            {
                MysteryDice.CustomLogger.LogError("AINodes not found. Ensure they are tagged correctly.");
            }
            SceneManager.MoveGameObjectToScene(this.gameObject, RoundManager.Instance.mapPropsContainer.scene);
        }   
        public void Initialize(SmartAgentNavigator assignedAgent)
        {
            agent = assignedAgent;
            agent.SetAllValues(true);
            agent.OnUseEntranceTeleport.AddListener(OnUseEntranceTeleport);
            agent.OnEnableOrDisableAgent.AddListener(OnEnableOrDisableAgent);
           
        }
        public void OnEnableOrDisableAgent(bool agentEnabled)
        {
            
        }

        public void OnUseEntranceTeleport(bool setOutside)
        {
            
        }

        public void FixedUpdate()
        {
            if (Networker.Instance.IsServer)
            {
                if (follows != null && agent != null)
                {
                    agent.AdjustSpeedBasedOnDistance(1.5f);
                    if (StartOfRound.Instance.inShipPhase) follows = null;
                }

                _pathTimer -= Time.fixedDeltaTime;

                if (_pathTimer <= 0f)
                {
                    if (follows != null)
                    {
                        _currentTargetPosition = follows.transform.position; // Update target position periodically
                        _currentIsInside = follows.isInsideFactory;
                    }
                    else
                    {
                        bool isInside = false;
                        Vector3 targetPosition = Vector3.zero;
                        _pathTimer = UnityEngine.Random.Range(5f, 25f);
                        var e = UnityEngine.Random.value;

                        if (e > 0.5f)
                        {
                            isInside = false;
                            targetPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(
                                outsidePoints[UnityEngine.Random.Range(0, outsidePoints.Length)].transform.position +
                                new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f,
                                    UnityEngine.Random.Range(-10f, 10f))
                            );
                        }
                        else
                        {
                            isInside = true;
                            targetPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(
                                InsidePoints[UnityEngine.Random.Range(0, InsidePoints.Length)].transform.position +
                                new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f,
                                    UnityEngine.Random.Range(-10f, 10f))
                            );
                        }

                        if (agent == null)
                        {
                            return;
                        }

                        _currentTargetPosition = targetPosition;
                        _currentIsInside = isInside;
                    }
                }
                if (agent != null)
                {
                    agent.DoPathingToDestination(_currentTargetPosition);
                }
            }
        }
    }
}
