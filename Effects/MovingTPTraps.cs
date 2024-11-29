using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using System.Reflection;

namespace MysteryDice.Effects
{
    internal class MovingTPTraps : IEffect
    {
        public string Name => "Moving Teleporter Traps";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "The Teleporters are now Roombas";

        public void Use()
        {
            Networker.Instance.MovingTrapsInitServerRPC();
        }

        // public class TeleporterTrapMovement : MonoBehaviour
        // {
        //     Vector3 EndPosition = Vector3.zero;
        //     Vector3 GoingToPoint = Vector3.zero;
        //     public Vector3 TargetPosition = Vector3.zero;
        //     public Vector3[] paths = new Vector3[] { };
        //     NavMeshPath path;
        //
        //     const float DistanceTolerance = 1f;
        //     const float NewPathTime = 3f;
        //     float PathTimer = 0f;
        //     public int BlockedID = 0;
        //     public float MoveSpeed = 5f;
        //
        //     public object TeleporterTrapScr; // Using object for reflection-based access
        //
        //     void Start()
        //     {
        //         path = new NavMeshPath();
        //     }
        //
        //     public void FixedUpdate()
        //     {
        //         // Use reflection to check if TeleporterTrapScr has triggered
        //         if (CheckIfTriggered())
        //         {
        //             Destroy(this);
        //             return;
        //         }
        //
        //         if (Networker.Instance.IsServer)
        //         {
        //             PathTimer -= Time.fixedDeltaTime;
        //
        //             if (PathTimer <= 0f)
        //             {
        //                 PathTimer = NewPathTime;
        //                 GameObject[] ainodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
        //                 TargetPosition = ainodes[UnityEngine.Random.Range(0, ainodes.Length)].transform.position;
        //                 CalculateNewPath();
        //                 MoveSpeed = UnityEngine.Random.Range(3f, 7f);
        //                 Networker.Instance.SyncDataTPClientRPC(GetNetworkObjectId(), MoveSpeed, transform.position, TargetPosition, BlockedID);
        //             }
        //         }
        //
        //         if (paths.Length == 0) return;
        //
        //         float distance = Vector3.Distance(transform.position, GoingToPoint);
        //         float endDistance = Vector3.Distance(transform.position, EndPosition);
        //
        //         Vector3 direction = (GoingToPoint - transform.position).normalized;
        //
        //         if (distance < DistanceTolerance)
        //             GetNewPath();
        //         else
        //             transform.position = transform.position + direction * MoveSpeed * Time.fixedDeltaTime;
        //
        //
        //         if (endDistance < DistanceTolerance)
        //         {
        //             TriggerTrap();
        //             Destroy(this.gameObject);
        //         }
        //     }
        //
        //     public void CalculateNewPath()
        //     {
        //         NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, path);
        //
        //         paths = path.corners.ToArray<Vector3>();
        //
        //         if (paths.Length == 0)
        //         {
        //             NavMesh.CalculatePath(transform.position + Vector3.up * 3f, TargetPosition, NavMesh.AllAreas, path);
        //             paths = path.corners.ToArray<Vector3>();
        //         }
        //
        //         BlockedID = 0;
        //         GetNewPath();
        //     }
        //
        //     void GetNewPath()
        //     {
        //         for (int i = 0; i < paths.Length; i++)
        //         {
        //             if (Vector3.Distance(paths[i], transform.position) > DistanceTolerance && i > BlockedID)
        //             {
        //                 BlockedID = i;
        //                 GoingToPoint = paths[i];
        //                 break;
        //             }
        //         }
        //     }
        //
        //     private bool CheckIfTriggered()
        //     {
        //         Type teleporterTrapType = TeleporterTrapScr.GetType();
        //         PropertyInfo hasTriggeredProp = teleporterTrapType.GetProperty("hasTriggered", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //         if (hasTriggeredProp != null)
        //         {
        //             return (bool)hasTriggeredProp.GetValue(TeleporterTrapScr);
        //         }
        //         return false;
        //     }
        //
        //     private ulong GetNetworkObjectId()
        //     {
        //         Type teleporterTrapType = TeleporterTrapScr.GetType();
        //         PropertyInfo networkObjectIdProp = teleporterTrapType.GetProperty("NetworkObjectId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //         if (networkObjectIdProp != null)
        //         {
        //             return (ulong)networkObjectIdProp.GetValue(TeleporterTrapScr);
        //         }
        //         return 0;
        //     }
        //
        //     private void TriggerTrap()
        //     {
        //         Type teleporterTrapType = TeleporterTrapScr.GetType();
        //         MethodInfo triggerTrapMethod = teleporterTrapType.GetMethod("TriggerTrap", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //         if (triggerTrapMethod != null)
        //         {
        //             triggerTrapMethod.Invoke(TeleporterTrapScr, null);
        //         }
        //     }
        // }
    }
}
