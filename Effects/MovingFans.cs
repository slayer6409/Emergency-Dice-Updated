using System;
using System.Collections;
using System.Collections.Generic;
using DunGen;
using System.Linq;
using CodeRebirth.src.Content.Maps;
using MysteryDice.Patches;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
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
        public static IEnumerator WaitForTrapInit(string trapName)
        {
            yield return new WaitForSeconds(5f);
            if (!RoundManager.Instance.IsHost) yield break;
            
            foreach (var trap in GameObject.FindObjectsOfType<GameObject>().Where(c => c.name.ToUpper().Contains(trapName.ToUpper())))
            {
                if(trap.name.Contains("Gal")) continue;
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
                    agent.GetComponent<NetworkObject>().Spawn(destroyWithScene:true);
                    Networker.Instance.setParentServerRPC(trap.GetComponent<NetworkObject>().NetworkObjectId, agent.GetComponent<NetworkObject>().NetworkObjectId);
                    targetObject.transform.localPosition = Vector3.zero;
                    targetObject.transform.localRotation = Quaternion.identity;
                    mm.Initialize(smartAgentNavigator);
                    mm.enabled = true;
                }
            }
        }
    }

    
    public class MakeMove : MonoBehaviour
    { 
        private float _pathTimer = 0f;
        

        private GameObject[] outsidePoints;
        private GameObject[] InsidePoints;
        public SmartAgentNavigator agent;
        private void Awake()
        {
            
            outsidePoints = RoundManager.Instance.outsideAINodes;
            InsidePoints = RoundManager.Instance.insideAINodes;

            if (outsidePoints.Length == 0 || InsidePoints.Length == 0)
            {
                MysteryDice.CustomLogger.LogError("AINodes not found. Ensure they are tagged correctly.");
            }
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
            
            // if (StartOfRound.Instance.inShipPhase)
            // {
            //     if(StartOfRound.Instance.IsHost) if(gameObject.transform.parent.GetComponent<NetworkObject>() != null) gameObject.transform.parent.GetComponent<NetworkObject>().Despawn();
            // }
            try
            {            
                if (Networker.Instance.IsServer)
                {
                    _pathTimer -= Time.fixedDeltaTime;
                
                    if (agent == null)
                    {
                        //Debug.LogError($"SmartAgentNavigator is null on {gameObject.name}. Ensure it is properly initialized.");
                        Destroy(this);
                        return;
                    }
                    if (_pathTimer <= 0f)
                    {
                        bool isInside = false;
                        Vector3 TargetPosition = Vector3.zero;
                        _pathTimer = UnityEngine.Random.Range(10f, 25f);
                        var e = UnityEngine.Random.value;
                        if (e > 0.5f)
                        {
                            isInside = false;
                            //Debug.LogWarning("Moving outside");
                            TargetPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(outsidePoints[UnityEngine.Random.Range(0, outsidePoints.Length)].transform.position + new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, UnityEngine.Random.Range(-10f, 10f)));
                        }
                        else
                        {
                            isInside = true;
                            //Debug.LogWarning("Moving inside");
                            TargetPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(InsidePoints[UnityEngine.Random.Range(0, InsidePoints.Length)].transform.position + new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, UnityEngine.Random.Range(-10f, 10f)));
                        }
                        if (agent == null)
                        {
                            //Debug.LogError($"SmartAgent component not found on GameObject {transform.parent.name}.");
                            return;
                        }
                   
                        Networker.Instance.MoveTrapServerRpc(agent.GetInstanceID(), TargetPosition, isInside);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                //This is a very stupid way to do it, but it works
                //don't look at this code
                foreach (Transform child in transform)
                {
                    //MysteryDice.CustomLogger.LogWarning("Deleting child: " + child.name + "");
                    var neto = child.gameObject.GetComponent<NetworkObject>();
                    if (neto!=null)neto.Despawn();
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
                //MysteryDice.CustomLogger.LogWarning("Deleting parent: " + transform.parent.name + "");
                transform.parent.GetComponent<NetworkObject>().Despawn();
                if (transform.parent.name.StartsWith("Agent"))
                {
                    try
                    {
                        Destroy(transform.parent.gameObject);
                    }
                    catch
                    {
                        
                    }
                }
                Destroy(this.gameObject);
            }
        }
    }
}
