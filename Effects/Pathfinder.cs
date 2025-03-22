using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class Pathfinder : IEffect
    {
        public string Name => "Pathfinder";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You've got a pathfinder!";
        public void Use()
        {
            Networker.Instance.PathfinderGiveSpawnerServerRPC(GameNetworkManager.Instance.localPlayerController.actualClientId);
        }

        public static void GiveBlobItem(ulong userID)
        {

            GameObject obj = UnityEngine.Object.Instantiate(MysteryDice.PathfinderSpawner.spawnPrefab,
                   Misc.GetPlayerByUserID(userID).transform.position,
                   Quaternion.identity,
                   RoundManager.Instance.playersManager.propsContainer);

            obj.GetComponent<GrabbableObject>().fallTime = 0f;
            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
            obj.GetComponent<GrabbableObject>().EnablePhysics(true);
        }

        public static void SpawnBlobs()
        {
            foreach(PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!Misc.IsPlayerAliveAndControlled(player)) continue;
                if (!player.isInsideFactory) continue;

                GameObject obj = UnityEngine.Object.Instantiate(MysteryDice.PathfinderPrefab,
                   player.transform.position,
                   Quaternion.identity,
                   RoundManager.Instance.playersManager.propsContainer);

                obj.GetComponent<NetworkObject>().Spawn();
            }
        }

        public class BlobspawnerBehaviour : PhysicsProp
        {
            GameObject BlobModel = null;
            public override void Start()
            {
                base.Start();
                BlobModel = gameObject.transform.Find("Model").gameObject;
            }

            public override void PocketItem()
            {
                BlobModel.SetActive(false);
                base.PocketItem();

            }
            public override void EquipItem()
            {
                BlobModel.SetActive(true);
                base.EquipItem();
            }

            public override void OnHitGround()
            {
                base.OnHitGround();
                BlobModel.SetActive(true);
            }

            public override void ItemActivate(bool used, bool buttonDown = true)
            {
                base.ItemActivate(used, buttonDown);
                if (buttonDown)
                {
                    if (StartOfRound.Instance == null) return;
                    if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

                    Networker.Instance.PathfinderSpawnBlobsServerRPC();
                    this.DestroyObjectInHand(playerHeldBy);
                }
            }
        }

        public class PathfindBehaviour : MonoBehaviour
        {
            Vector3 EndPosition = Vector3.zero;
            Vector3 GoingToPoint = Vector3.zero;
            Vector3 PositionOffset = new Vector3(0f, 1f, 0f);

            const float MaxVelocity = 2f;
            const float DistanceTolerance = 1f;
            float MoveVelocity = 2f;
            float Timer = 1f;

            int BlockedID = 0;

            NavMeshPath path;

            void Start()
            {
                EntranceTeleport[] exits = GameObject.FindObjectsOfType<EntranceTeleport>(false);
                foreach(EntranceTeleport exit in  exits)
                {
                    if (!exit.isEntranceToBuilding && exit.entranceId == 0)
                    {
                        EndPosition = exit.entrancePoint.position;
                        break;
                    }
                }
                path = new NavMeshPath();
            }
            void FixedUpdate()
            {
                Timer -= Time.fixedDeltaTime;
                if (Timer < 0f)
                {
                    NavMesh.CalculatePath(transform.position-PositionOffset, EndPosition, NavMesh.AllAreas, path);
                    BlockedID = 0;
                    Timer = 1f;
                    GetNewPath();
                }

                float distance = Vector3.Distance(transform.position - PositionOffset, GoingToPoint);
                float endDistance = Vector3.Distance(transform.position - PositionOffset, EndPosition);

                Vector3 direction = (GoingToPoint - transform.position).normalized;

                if (distance < DistanceTolerance)
                    GetNewPath();
                else
                    transform.position = transform.position + direction * MoveVelocity * Time.fixedDeltaTime;
                    

                if (endDistance < DistanceTolerance)
                {
                    Landmine.SpawnExplosion(transform.position, true, 0, 0, 0, 0, null, false);
                    Destroy(this.gameObject);
                }
            }

            void GetNewPath()
            {
                for (int i = 0; i < path.corners.Length; i++)
                {
                    if (Vector3.Distance(path.corners[i], transform.position - PositionOffset) > DistanceTolerance && i > BlockedID)
                    {
                        BlockedID = i;
                        GoingToPoint = path.corners[i] + PositionOffset;
                        break;
                    }
                }
            }
        }
    }
}
