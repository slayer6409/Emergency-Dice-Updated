using DunGen;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class MovingLandmines : IEffect
    {
        public string Name => "Moving Landmines";

        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Landmines are now moving!";

        public void Use()
        {
            Networker.Instance.MovingMinesInitServerRPC();
        }

        public class LandmineMovement : MonoBehaviour
        {
            Vector3 EndPosition = Vector3.zero;
            Vector3 GoingToPoint = Vector3.zero;
            public Vector3 TargetPosition = Vector3.zero;
            public Vector3[] paths = new Vector3[] { };
            NavMeshPath path;

            const float DistanceTolerance = 1f;
            const float NewPathTime = 3f;
            float PathTimer = 0f;
            public int BlockedID = 0;
            public float MoveSpeed = 5f;

            public Landmine LandmineScr;

            void Start()
            {
                path = new NavMeshPath();
            }

            public void FixedUpdate()
            {
                if (LandmineScr.hasExploded)
                {
                    Destroy(this);
                    return;
                }

                if(Networker.Instance.IsServer)
                {
                    PathTimer -= Time.fixedDeltaTime;

                    if (PathTimer <= 0f)
                    {
                        PathTimer = NewPathTime;
                        GameObject[] ainodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                        TargetPosition = ainodes[UnityEngine.Random.Range(0, ainodes.Length)].transform.position;
                        CalculateNewPath();
                        MoveSpeed = UnityEngine.Random.Range(3f, 7f);
                        Networker.Instance.SyncDataClientRPC(LandmineScr.NetworkObjectId,MoveSpeed, transform.position, TargetPosition, BlockedID);
                    }
                }

                if (paths.Length == 0) return;

                float distance = Vector3.Distance(transform.position, GoingToPoint);
                float endDistance = Vector3.Distance(transform.position, EndPosition);

                Vector3 direction = (GoingToPoint - transform.position).normalized;

                if (distance < DistanceTolerance)
                    GetNewPath();
                else
                    transform.position = transform.position + direction * MoveSpeed * Time.fixedDeltaTime;


                if (endDistance < DistanceTolerance)
                {
                    Landmine.SpawnExplosion(transform.position, true, 0, 0, 0, 0, null, false);
                    Destroy(this.gameObject);
                }
            }

            

            public void CalculateNewPath()
            {
                NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, path);

                paths = path.corners.ToArray<Vector3>();

                if(paths.Length == 0)
                {
                    NavMesh.CalculatePath(transform.position + Vector3.up*3f,TargetPosition, NavMesh.AllAreas, path);
                    paths = path.corners.ToArray<Vector3>();
                }

                BlockedID = 0;
                GetNewPath();
            }

            void GetNewPath()
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    if (Vector3.Distance(paths[i], transform.position ) > DistanceTolerance && i > BlockedID)
                    {
                        BlockedID = i;
                        GoingToPoint = paths[i];
                        break;
                    }
                }
            }
        }
    }
}
