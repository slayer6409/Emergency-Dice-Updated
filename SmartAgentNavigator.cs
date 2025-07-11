// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using GameNetcodeStuff;
// using MysteryDice.Patches;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.AI;
// using UnityEngine.Events;
//
// namespace MysteryDice;
// [RequireComponent(typeof(NavMeshAgent))]
// public class OldSmartAgentNavigator : NetworkBehaviour
// {
//     [HideInInspector] public bool cantMove = false;
//     [HideInInspector] public UnityEvent<bool> OnUseEntranceTeleport = new();
//     [HideInInspector] public UnityEvent<bool> OnEnableOrDisableAgent = new();
//     [HideInInspector] public UnityEvent<bool> OnEnterOrExitElevator = new();
//
//     private float nonAgentMovementSpeed = 10f;
//     [HideInInspector] public NavMeshAgent agent = null!;
//     private Vector3 pointToGo = Vector3.zero;
//     [HideInInspector] public bool isOutside = true;
//     private bool usingElevator = false;
//     private bool InElevator => elevatorScript != null && Vector3.Distance(this.transform.position, elevatorScript.elevatorInsidePoint.position) < 7f;
//     private bool wasInElevatorLastFrame = false;
//     private Coroutine? searchRoutine = null;
//     private Coroutine? searchCoroutine = null;
//     private bool isSearching = false;
//     private bool reachedDestination = false;
//     private MineshaftElevatorController? elevatorScript = null;
//     [HideInInspector] public EntranceTeleport? entranceToUse = null;
//     [HideInInspector] public List<EntranceTeleport> exitPoints = new();
//
//     public void Start()
//     {
//         agent = gameObject.GetComponent<NavMeshAgent>(); 
//         PlayerControllerBPatch.smartAgentNavigators.Add(this);
//     }
//
//     public override void OnNetworkDespawn()
//     {
//         base.OnNetworkDespawn();
//         PlayerControllerBPatch.smartAgentNavigators.Remove(this);
//     }
//
//     public void SetAllValues(bool isOutside)
//     {
//         this.isOutside = isOutside;
//
//         exitPoints.Clear();
//         foreach (var exit in FindObjectsByType<EntranceTeleport>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID))
//         {
//             exitPoints.Add(exit);
//             if (!exit.FindExitPoint())
//             {
//                 MysteryDice.CustomLogger.LogError("Something went wrong in the generation of the fire exits");
//             }
//         }
//         elevatorScript = RoundManager.Instance.currentMineshaftElevator;
//     }
//
//     public void ResetAllValues()
//     {
//         exitPoints.Clear();
//         elevatorScript = null;
//     }
//     
//     public void Update()
//     {
//         if (InElevator)
//         {
//             if (!wasInElevatorLastFrame)
//             {
//                 OnEnterOrExitElevator.Invoke(true);
//             }
//             wasInElevatorLastFrame = true;
//         }
//         else if (wasInElevatorLastFrame)
//         {
//             OnEnterOrExitElevator.Invoke(false);
//             wasInElevatorLastFrame = false;
//         }
//     }
//
//     public bool DoPathingToDestination(Vector3 destination, bool destinationIsInside)
//     {
//         if (!agent.enabled)
//         {
//             if (cantMove) return true;
//             Vector3 targetPosition = pointToGo;
//             float arcHeight = 10f;  // Adjusted arc height for a more pronounced arc
//             float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
//
//             // Calculate the new position in an arcing motion
//             float normalizedDistance = Mathf.Clamp01(Vector3.Distance(transform.position, targetPosition) / distanceToTarget);
//             Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * nonAgentMovementSpeed);
//             newPosition.y += Mathf.Sin(normalizedDistance * Mathf.PI) * arcHeight;
//
//             transform.position = newPosition;
//             transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
//             if (Vector3.Distance(transform.position, targetPosition) <= 1f)
//             {
//                 OnEnableOrDisableAgent.Invoke(true);
//                 agent.enabled = true;
//             }
//             return true;
//         }
//
//         if ((isOutside && destinationIsInside) || (!isOutside && !destinationIsInside))
//         {
//             GoThroughEntrance(destination);
//             return true;
//         }
//
//         entranceToUse = null;
//         if (!isOutside && elevatorScript != null && !usingElevator)
//         {
//             bool scriptCloserToTop = Vector3.Distance(transform.position, elevatorScript.elevatorTopPoint.position) < Vector3.Distance(transform.position, elevatorScript.elevatorBottomPoint.position);
//             bool destinationCloserToTop = Vector3.Distance(destination, elevatorScript.elevatorTopPoint.position) < Vector3.Distance(destination, elevatorScript.elevatorBottomPoint.position);
//             if (scriptCloserToTop != destinationCloserToTop)
//             {
//                 UseTheElevator(elevatorScript);
//                 return true;
//             }
//         }
//
//         bool playerIsInElevator = elevatorScript != null && !elevatorScript.elevatorFinishedMoving && Vector3.Distance(destination, elevatorScript.elevatorInsidePoint.position) < 7f;
//         if (!usingElevator && !playerIsInElevator)
//         {
//             if (DetermineIfNeedToDisableAgent(destination))
//             {
//                 return true;
//             }
//         }
//
//         if (!usingElevator)
//         {
//             agent.SetDestination(destination);
//         }
//         if (usingElevator && elevatorScript != null) agent.Warp(elevatorScript.elevatorInsidePoint.position);
//         return false;
//     }
//
//     private bool DetermineIfNeedToDisableAgent(Vector3 destination)
//     {
//         if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 5f, NavMesh.AllAreas))
//         {
//             return false;
//         }
//
//         Vector3 finalDestination = hit.position;
//         float distanceToDest = Vector3.Distance(transform.position, finalDestination);
//         if (distanceToDest < agent.stoppingDistance)
//         {
//             return false;
//         }
//
//         NavMeshPath path = new NavMeshPath();
//         bool pathFound = agent.CalculatePath(finalDestination, path);
//         if (!pathFound || path.status != NavMeshPathStatus.PathComplete)
//         {
//             Vector3 lastValidPoint = FindClosestValidPoint();
//             agent.SetDestination(lastValidPoint);
//             if (Vector3.Distance(agent.transform.position, lastValidPoint) <= agent.stoppingDistance)
//             {
//                 pointToGo = finalDestination;
//                 OnEnableOrDisableAgent.Invoke(false);
//                 agent.enabled = false;
//                 
//                 return true;
//             }
//         }
//
//         return false;
//     }
//
//     public float CanPathToPoint(Vector3 startPos, Vector3 endPos)
//     {
//         // Calculate the path
//         NavMeshPath path = new NavMeshPath();
//         bool pathFound = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);
//
//         // If we failed to calculate a path or it’s incomplete/invalid, return false
//         if (!pathFound || path.status != NavMeshPathStatus.PathComplete)
//         {
//             return -1;
//         }
//
//         // If you also want to check the path distance, you can do something like:
//         float pathDistance = 0f;
//         if (path.corners.Length > 1)
//         {
//             for (int i = 1; i < path.corners.Length; i++)
//             {
//                 pathDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
//             }
//         }
//
//         return pathDistance;
//     }
//
//     public EntranceTeleport? FindViableEntranceToPath(Vector3 endPosition)
//     {
//         Dictionary<EntranceTeleport, float> validExitPoints = new();
//         foreach (var entrance in exitPoints)
//         {
//             if (entrance == null || !entrance.isEntranceToBuilding) 
//                 continue;
//
//             // Agent Navigator -> Entrance
//             float canEntranceReachTransporter = CanPathToPoint(
//                 isOutside ? entrance.entrancePoint.position : entrance.exitPoint.position, 
//                 transform.position
//             );
//             if (canEntranceReachTransporter < 0) 
//                 continue;
//
//             // Entrance -> End Position
//             float canObjectReachEntrance = CanPathToPoint(
//                 isOutside ? entrance.exitPoint.position : entrance.entrancePoint.position, 
//                 endPosition
//             );
//             if (canObjectReachEntrance < 0)
//                 continue;
//
//             // If both legs are good, entrance is "viable"
//             validExitPoints.Add(entrance, canEntranceReachTransporter + canObjectReachEntrance);
//         }
//
//         if (validExitPoints.Count > 0)
//         {
//             float bestDistance = float.MaxValue;
//             EntranceTeleport? bestTeleport = null;
//             foreach (var exitPoint in validExitPoints)
//             {
//                 if (exitPoint.Value < bestDistance)
//                 {
//                     bestDistance = exitPoint.Value;
//                     bestTeleport = exitPoint.Key;
//                 }
//             }
//             return bestTeleport;
//         }
//         return null;
//     }
//
//     public void GoThroughEntrance(Vector3 actualEndPosition)
//     {
//         // Attempt to find an entrance that’s viable for (object -> entrance) and (entrance -> agent).
//         if (entranceToUse == null)
//         {
//             entranceToUse = FindViableEntranceToPath(actualEndPosition);
//         }
//         if (entranceToUse == null) // still null
//         {
//             // Fallback: maybe use lastUsedEntranceTeleport or do nothing
//             return;
//         }
//
//         var viableOutsideEntrance = entranceToUse;
//         Vector3 destination;
//         Vector3 destinationAfterTeleport;
//
//         if (isOutside)
//         {
//             destination = viableOutsideEntrance.entrancePoint.position; // walk here
//             destinationAfterTeleport = viableOutsideEntrance.exitPoint.position; // appear here
//         }
//         else
//         {
//             destination = viableOutsideEntrance.exitPoint.position;
//             destinationAfterTeleport = viableOutsideEntrance.entrancePoint.position;
//         }
//
//         if (elevatorScript != null && NeedsElevator(destination, viableOutsideEntrance, elevatorScript))
//         {
//             UseTheElevator(elevatorScript);
//             return;
//         }
//
//         float distanceToDestination = Vector3.Distance(transform.position, destination);
//         if (distanceToDestination <= agent.stoppingDistance)
//         {
//             entranceToUse = null;
//             agent.Warp(destinationAfterTeleport);
//             SetThingOutsideServerRpc(!isOutside);
//         }
//         else
//         {
//             // Otherwise, set the path to that entrance
//             agent.SetDestination(destination);
//         }
//     }
//
//     private bool NeedsElevator(Vector3 destination, EntranceTeleport entranceTeleportToUse, MineshaftElevatorController elevatorScript)
//     {
//         // Determine if the elevator is needed based on destination proximity and current position
//         bool nearMainEntrance = Vector3.Distance(destination, RoundManager.FindMainEntrancePosition(true, false)) < Vector3.Distance(destination, entranceTeleportToUse.transform.position);
//         bool closerToTop = Vector3.Distance(transform.position, elevatorScript.elevatorTopPoint.position) < Vector3.Distance(transform.position, elevatorScript.elevatorBottomPoint.position);
//         return !isOutside && ((nearMainEntrance && !closerToTop) || (!nearMainEntrance && closerToTop));
//     }
//
//     private void UseTheElevator(MineshaftElevatorController elevatorScript)
//     {
//         // Determine if we need to go up or down based on current position and destination
//         bool goUp = Vector3.Distance(transform.position, elevatorScript.elevatorBottomPoint.position) < Vector3.Distance(transform.position, elevatorScript.elevatorTopPoint.position);
//         // Check if the elevator is finished moving
//         if (elevatorScript.elevatorFinishedMoving)
//         {
//             if (elevatorScript.elevatorDoorOpen)
//             {
//                 // If elevator is not called yet and is at the wrong level, call it
//                 if (NeedToCallElevator(elevatorScript, goUp))
//                 {
//                     elevatorScript.CallElevatorOnServer(goUp);
//                     MoveToWaitingPoint(elevatorScript, goUp);
//                     return;
//                 }
//                 // Move to the inside point of the elevator if not already there
//                 if (Vector3.Distance(transform.position, elevatorScript.elevatorInsidePoint.position) > 1f)
//                 {
//                     agent.SetDestination(elevatorScript.elevatorInsidePoint.position);
//                 }
//                 else if (!usingElevator)
//                 {
//                     // Press the button to start moving the elevator
//                     elevatorScript.PressElevatorButtonOnServer(true);
//                     StartCoroutine(StopUsingElevator(elevatorScript));
//                 }
//             }
//         }
//         else
//         {
//             MoveToWaitingPoint(elevatorScript, goUp);
//         }
//     }
//
//     private IEnumerator StopUsingElevator(MineshaftElevatorController elevatorScript)
//     {
//         usingElevator = true;
//         yield return new WaitForSeconds(2f);
//         yield return new WaitUntil(() => elevatorScript.elevatorDoorOpen && elevatorScript.elevatorFinishedMoving);
//         usingElevator = false;
//     }
//
//     private bool NeedToCallElevator(MineshaftElevatorController elevatorScript, bool needToGoUp)
//     {
//         return !elevatorScript.elevatorCalled && ((!elevatorScript.elevatorIsAtBottom && needToGoUp) || (elevatorScript.elevatorIsAtBottom && !needToGoUp));
//     }
//
//     private void MoveToWaitingPoint(MineshaftElevatorController elevatorScript, bool needToGoUp)
//     {
//         // Elevator is currently moving
//         // Move to the appropriate waiting point (bottom or top)
//         if (Vector3.Distance(transform.position, elevatorScript.elevatorInsidePoint.position) > 1f)
//         {
//             agent.SetDestination(needToGoUp ? elevatorScript.elevatorBottomPoint.position : elevatorScript.elevatorTopPoint.position);
//         }
//         else
//         {
//             // Wait at the inside point for the elevator to arrive
//             agent.SetDestination(elevatorScript.elevatorInsidePoint.position);
//         }
//     }
//
//     [ServerRpc(RequireOwnership = false)]
//     public void SetThingOutsideServerRpc(bool setOutside)
//     {
//         SetThingOutsideClientRpc(setOutside);
//     }
//
//     [ClientRpc]
//     public void SetThingOutsideClientRpc(bool setOutside)
//     {
//         isOutside = setOutside;
//         OnUseEntranceTeleport.Invoke(setOutside);
//     }
//
//     /// <summary>
//     /// Finds the closest valid point from a partial path.
//     /// </summary>
//     /// <returns>The closest valid point to continue the journey from.</returns>
//     private Vector3 FindClosestValidPoint()
//     {
//         return agent.pathEndPosition;
//     }
//
//     /// <summary>
//     /// Warps the agent forward in its current direction until it lands on a valid NavMesh.
//     /// </summary>
//     /// <param name="originalDestination">The target destination outside the navmesh.</param>
//     /// <returns>The point on the navmesh.</returns>
//     private Vector3 WarpForwardUntilOnNavMesh(Vector3 originalDestination)
//     {
//         Vector3 warpPoint = originalDestination;
//         Vector3 forwardDirection = agent.transform.forward; // Get the agent's current facing direction
//         float warpDistance = 0.2f; // The distance to warp forward each time
//         float maxWarpAttempts = 50; // Limit the number of warp attempts
//         float navMeshCheckDistance = 1f; // The radius to check for a valid NavMesh
//
//         for (int i = 0; i < maxWarpAttempts; i++)
//         {
//             Vector3 testPoint = warpPoint + forwardDirection * warpDistance * (i + 1);
//
//             // Check if this new point is on the NavMesh
//             if (NavMesh.SamplePosition(testPoint, out NavMeshHit hit, navMeshCheckDistance, NavMesh.AllAreas))
//             {
//                 // Found a valid point on the NavMesh
//                 return hit.position;
//             }
//         }
//
//         // If no valid point is found after multiple attempts, return zero vector
//         return Vector3.zero;
//     }
//
//     /// <summary>
//     /// Sets a destination and adjusts the agent's speed based on the distance to the target.
//     /// The farther the distance, the faster the agent moves.
//     /// </summary>
//     /// <param name="destination">The target destination.</param>
//     public void SetDestinationWithSpeedAdjustment(Vector3 destination)
//     {
//         agent.SetDestination(destination);
//         AdjustSpeedBasedOnDistance(agent.remainingDistance);
//     }
//
//     /// <summary>
//     /// Adjusts the agent's speed based on its remaining distance.
//     /// The farther the distance, the faster the agent moves.
//     /// </summary>
//     /// <param name="distance">The distance to the target.</param>
//     public void AdjustSpeedBasedOnDistance(float multiplierBoost)
//     {
//         float minDistance = 0f;
//         float maxDistance = 40f;
//
//         float minSpeed = 0f; // Speed when closest
//         float maxSpeed = 10f; // Speed when farthest
//
//         float clampedDistance = Mathf.Clamp(agent.remainingDistance, minDistance, maxDistance);
//         float normalizedDistance = (clampedDistance - minDistance) / (maxDistance - minDistance);
//
//         agent.speed = Mathf.Lerp(minSpeed, maxSpeed, normalizedDistance) * multiplierBoost;
//     }
//
//     /// <summary>
//     /// Cancels the current movement and stops the agent.
//     /// </summary>
//     public void StopNavigation()
//     {
//         if (agent != null)
//         {
//             agent.isStopped = true;
//             agent.ResetPath();
//         }
//     }
//
//     /// <summary>
//     /// Teleports the agent to a specified location.
//     /// </summary>
//     /// <param name="location">The target location to warp to.</param>
//     public void WarpToLocation(Vector3 location)
//     {
//         agent.Warp(location);
//     }
//
//     public bool CurrentPathIsValid()
//     {
//         if (agent.path.status == NavMeshPathStatus.PathPartial || agent.path.status == NavMeshPathStatus.PathInvalid)
//         {
//             return false;
//         }
//         return true;
//     }
//
//     public void StartSearchRoutine(Vector3 position, float radius)
//     {
//         if (searchRoutine != null)
//         {
//             StopCoroutine(searchRoutine);
//         }
//         isSearching = true;
//         searchRoutine = StartCoroutine(SearchAlgorithm(position, radius));
//     }
//
//     public void StopSearchRoutine()
//     {
//         isSearching = false;
//         if (searchCoroutine != null)
//         {
//             StopCoroutine(searchRoutine);
//         }
//         searchRoutine = null;
//     }
//
//     private IEnumerator SearchAlgorithm(Vector3 position, float radius)
//     {
//         while (isSearching)
//         {
//             Vector3 positionToTravel = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, radius, default);
//             reachedDestination = false;
//
//             while (!reachedDestination && isSearching)
//             {
//                 agent.SetDestination(positionToTravel);
//                 yield return new WaitForSeconds(3f);
//
//                 if (Vector3.Distance(this.transform.position, positionToTravel) <= 10f || agent.velocity.magnitude <= 1f)
//                 {
//                     reachedDestination = true;
//                 }
//             }
//         }
//
//         searchRoutine = null; // Clear the coroutine reference when it finishes
//     }
// }