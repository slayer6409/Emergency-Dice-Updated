using System.Collections;
using DunGen;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MovingSeaTraps : IEffect
    {
        public string Name => "Unchained";

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Who cut their chain??";

        public void Use()
        {
            Networker.Instance.MovingSeatrapsServerRPC();
        }
        // public static IEnumerator WaitForSeaTrapInit()
        // {
        //     yield return new WaitForSeconds(5f);
        //     foreach (Bertha mine in GameObject.FindObjectsOfType<Bertha>())
        //     {
        //         if (mine.transform.parent.gameObject.GetComponent<SmartAgent>() == null)
        //         {
        //             mine.transform.parent.gameObject.AddComponent<SmartAgent>();    
        //         }
        //     }
        //
        //     foreach (var mine in GameObject.FindObjectsOfType<Seamine>())
        //     {
        //         if (mine.transform.parent.gameObject.GetComponent<SmartAgent>() == null)
        //         {
        //             mine.transform.parent.gameObject.AddComponent<SmartAgent>();    
        //             
        //         }
        //     }
    //     // }
    //     public class SeamineMovement : MonoBehaviour
    //     {
    //         Vector3 EndPosition = Vector3.zero;
    //         Vector3 GoingToPoint = Vector3.zero;
    //         public Vector3 TargetPosition = Vector3.zero;
    //         public Vector3[] paths = new Vector3[] { };
    //         NavMeshPath path;
    //
    //         const float DistanceTolerance = 1f;
    //         const float NewPathTime = 3f;
    //         float PathTimer = 0f;
    //         public int BlockedID = 0;
    //         public float MoveSpeed = 5f;
    //
    //         public Seamine seamine;
    //
    //         void Start()
    //         {
    //             path = new NavMeshPath();
    //         }
    //
    //         public void FixedUpdate()
    //         {
    //             if (seamine.hasExploded)
    //             {
    //                 Destroy(this);
    //                 return;
    //             }
    //
    //             if(Networker.Instance.IsServer)
    //             {
    //                 PathTimer -= Time.fixedDeltaTime;
    //
    //                 if (PathTimer <= 0f)
    //                 {
    //                     PathTimer = NewPathTime;
    //                     GameObject[] ainodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    //                     TargetPosition = ainodes[UnityEngine.Random.Range(0, ainodes.Length)].transform.position;
    //                     CalculateNewPath();
    //                     MoveSpeed = UnityEngine.Random.Range(3f, 7f);
    //                     Networker.Instance.SyncSeaTrapDataClientRPC(seamine.NetworkObjectId,MoveSpeed, transform.position, TargetPosition, BlockedID);
    //                 }
    //             }
    //
    //             if (paths.Length == 0) return;
    //
    //             float distance = Vector3.Distance(transform.position, GoingToPoint);
    //             float endDistance = Vector3.Distance(transform.position, EndPosition);
    //
    //             Vector3 direction = (GoingToPoint - transform.position).normalized;
    //
    //             if (distance < DistanceTolerance)
    //                 GetNewPath();
    //             else
    //                 transform.position = transform.position + direction * MoveSpeed * Time.fixedDeltaTime;
    //
    //         }
    //
    //         
    //
    //         public void CalculateNewPath()
    //         {
    //             NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, path);
    //
    //             paths = path.corners.ToArray<Vector3>();
    //
    //             if(paths.Length == 0)
    //             {
    //                 NavMesh.CalculatePath(transform.position + Vector3.up*3f,TargetPosition, NavMesh.AllAreas, path);
    //                 paths = path.corners.ToArray<Vector3>();
    //             }
    //
    //             BlockedID = 0;
    //             GetNewPath();
    //         }
    //
    //         void GetNewPath()
    //         {
    //             for (int i = 0; i < paths.Length; i++)
    //             {
    //                 if (Vector3.Distance(paths[i], transform.position ) > DistanceTolerance && i > BlockedID)
    //                 {
    //                     BlockedID = i;
    //                     GoingToPoint = paths[i];
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //     public class BerthaMovement : MonoBehaviour
    //     {
    //         Vector3 EndPosition = Vector3.zero;
    //         Vector3 GoingToPoint = Vector3.zero;
    //         public Vector3 TargetPosition = Vector3.zero;
    //         public Vector3[] paths = new Vector3[] { };
    //         NavMeshPath path;
    //
    //         const float DistanceTolerance = 1f;
    //         const float NewPathTime = 3f;
    //         float PathTimer = 0f;
    //         public int BlockedID = 0;
    //         public float MoveSpeed = 5f;
    //
    //         public Bertha bigbertha;
    //
    //         void Start()
    //         {
    //             path = new NavMeshPath();
    //         }
    //
    //         public void FixedUpdate()
    //         {
    //             if (bigbertha.hasExploded)
    //             {
    //                 Destroy(this);
    //                 return;
    //             }
    //
    //             if(Networker.Instance.IsServer)
    //             {
    //                 PathTimer -= Time.fixedDeltaTime;
    //
    //                 if (PathTimer <= 0f)
    //                 {
    //                     PathTimer = NewPathTime;
    //                     GameObject[] ainodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    //                     TargetPosition = ainodes[UnityEngine.Random.Range(0, ainodes.Length)].transform.position;
    //                     CalculateNewPath();
    //                     MoveSpeed = UnityEngine.Random.Range(3f, 7f);
    //                     Networker.Instance.SyncSeaTrapDataClientRPC(bigbertha.NetworkObjectId,MoveSpeed, transform.position, TargetPosition, BlockedID);
    //                 }
    //             }
    //
    //             if (paths.Length == 0) return;
    //
    //             float distance = Vector3.Distance(transform.position, GoingToPoint);
    //             float endDistance = Vector3.Distance(transform.position, EndPosition);
    //
    //             Vector3 direction = (GoingToPoint - transform.position).normalized;
    //
    //             if (distance < DistanceTolerance)
    //                 GetNewPath();
    //             else
    //                 transform.position = transform.position + direction * MoveSpeed * Time.fixedDeltaTime;
    //
    //         }
    //
    //         
    //
    //         public void CalculateNewPath()
    //         {
    //             NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, path);
    //
    //             paths = path.corners.ToArray<Vector3>();
    //
    //             if(paths.Length == 0)
    //             {
    //                 NavMesh.CalculatePath(transform.position + Vector3.up*3f,TargetPosition, NavMesh.AllAreas, path);
    //                 paths = path.corners.ToArray<Vector3>();
    //             }
    //
    //             BlockedID = 0;
    //             GetNewPath();
    //         }
    //
    //         void GetNewPath()
    //         {
    //             for (int i = 0; i < paths.Length; i++)
    //             {
    //                 if (Vector3.Distance(paths[i], transform.position ) > DistanceTolerance && i > BlockedID)
    //                 {
    //                     BlockedID = i;
    //                     GoingToPoint = paths[i];
    //                     break;
    //                 }
    //             }
    //         }
    //     }
     }
}
