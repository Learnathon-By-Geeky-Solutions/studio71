using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace PatrolEnemy
{
    public class RecoveryState : IEnemyState
    {
        private Vector3 coverPoint;
        private bool hasCoverPoint = false;
        private float lastHealthRecoveryTime = 0f; // Declare the variable here
        private float healthRecoveryInterval = 0.5f; // You can adjust this interval

        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Recovery State");
            hasCoverPoint = false;
            controller.Agent.isStopped = false; // Ensure agent is moving
            controller.Agent.ResetPath();
            lastHealthRecoveryTime = Time.time; // Initialize on entering the state as well
        }

        public void UpdateState(EnemyController controller)
        {
            // If we don't have a cover point, find one
            if (!hasCoverPoint)
            {
                FindCoverPoint(controller);
                // lastHealthRecoveryTime is now initialized in EnterState and when cover is found
            }

            // Check if we've reached the cover point
            if (hasCoverPoint && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance && !controller.Agent.pathPending)
            {
                controller.Agent.isStopped = true; // Stop at the cover point

                // Recover health over time while at the cover point
                if (Time.time >= lastHealthRecoveryTime + healthRecoveryInterval)
                {
                    controller.CurrentHealth = Mathf.Min(controller.CurrentHealth + controller.RecoveryRate * Time.deltaTime, controller.MaxHealth);
                    lastHealthRecoveryTime = Time.time;
                    Debug.Log($"Recovering at cover. Current health: {controller.CurrentHealth}");

                    // If health is recovered, decide next state
                    if (controller.CurrentHealth >= controller.MaxHealth)
                    {
                        DecideNextState(controller);
                        return; // Exit UpdateState after full recovery and state decision
                    }
                }
            }
            else if (hasCoverPoint && controller.Agent.pathPending)
            {
                // Waiting for path calculation
            }
            else if (hasCoverPoint && controller.Agent.remainingDistance > controller.Agent.stoppingDistance)
            {
                // Moving towards cover
            }
            else if (hasCoverPoint && !controller.Agent.hasPath)
            {
                // Reached destination or path failed, start recovery (or decide next state if already recovered - unlikely here)
                controller.Agent.isStopped = true;
                if (controller.CurrentHealth >= controller.MaxHealth)
                {
                    DecideNextState(controller);
                    return;
                }
            }
        }

        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Recovery State");
            controller.Agent.isStopped = false; // Ensure agent can move again
        }

        private void FindCoverPoint(EnemyController controller)
        {
            // Find potential cover points around obstacles on the "Obstacle" layer
            int obstacleLayerMask = LayerMask.GetMask("Obstacle");
            Collider[] obstacles = Physics.OverlapSphere(controller.transform.position, controller.DetectionRange, obstacleLayerMask);
            List<Vector3> potentialCoverPoints = new List<Vector3>();

            if (controller.CurrentTarget == null)
            {
                // If no target, just try to move back to the initial position
                NavMeshHit hit;
                if (NavMesh.SamplePosition(controller.InitialPosition, out hit, 5f, NavMesh.AllAreas))
                {
                    coverPoint = hit.position;
                    controller.Agent.SetDestination(coverPoint);
                    hasCoverPoint = true;
                    Debug.Log($"No target, retreating to initial position: {coverPoint}");
                }
                else
                {
                    Debug.LogWarning("Could not find valid NavMesh point near initial position.");
                    hasCoverPoint = true; // To avoid infinite attempts
                }
                return;
            }

            foreach (Collider obstacle in obstacles)
            {
                // Get the direction from player to obstacle
                Vector3 playerToObstacle = obstacle.transform.position - controller.CurrentTarget.position;
                playerToObstacle.y = 0;
                playerToObstacle.Normalize();

                // Generate a point on the opposite side of the obstacle from the player
                Vector3 potentialPoint = obstacle.transform.position + playerToObstacle * 2f;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(potentialPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    potentialCoverPoints.Add(hit.position);
                }
            }

            // Find the closest valid cover point that blocks line of sight
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = controller.transform.position;
            bool foundValidCover = false;

            foreach (Vector3 point in potentialCoverPoints)
            {
                float distance = Vector3.Distance(controller.transform.position, point);

                // Check if this point is out of line of sight from player
                Vector3 directionToPlayer = (controller.CurrentTarget.position - point).normalized;
                RaycastHit hit;

                // NOW USING controller.obstacleLayer FOR THE RAYCAST
                if (Physics.Raycast(point, directionToPlayer, out hit, controller.DetectionRange, controller.obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    // If the raycast hits an object on the obstacle layer, it's a valid cover point
                    if (hit.collider != null)
                    {
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestPoint = point;
                            foundValidCover = true;
                        }
                    }
                }
            }

            // If we found a valid cover point, head there
            if (foundValidCover)
            {
                coverPoint = closestPoint;
                controller.Agent.SetDestination(coverPoint);
                hasCoverPoint = true;
                Debug.Log($"Moving to cover point: {coverPoint}");
            }
            else
            {
                // If no cover found on the obstacle layer, just move away from player
                if (controller.CurrentTarget != null)
                {
                    Vector3 awayFromPlayer = controller.transform.position - controller.CurrentTarget.position;
                    awayFromPlayer.y = 0;
                    awayFromPlayer.Normalize();

                    Vector3 retreatPoint = controller.transform.position + awayFromPlayer * 10f;

                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(retreatPoint, out navMeshHit, 10f, NavMesh.AllAreas))
                    {
                        coverPoint = navMeshHit.position;
                        controller.Agent.SetDestination(coverPoint);
                        hasCoverPoint = true;
                        Debug.Log($"No cover found on obstacle layer, retreating to: {coverPoint}");
                    }
                    else
                    {
                        hasCoverPoint = true; // To avoid infinite attempts
                        Debug.LogWarning("Could not find valid NavMesh point to retreat.");
                    }
                }
                else
                {
                    hasCoverPoint = true; // No target, nothing to move away from
                    Debug.Log("No target to retreat from.");
                }
            }
        }

        private void DecideNextState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);

            if (distanceToPlayer <= controller.AttackRange && controller.HasLineOfSight)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
            }
            else if (distanceToPlayer <= controller.AttackRange && controller.CurrentGrenades > 0 && !controller.HasLineOfSight)
            {
                controller.ChangeState(EnemyController.EnemyStateType.GrenadeThrow);
            }
            else if (distanceToPlayer <= controller.DetectionRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
            }
            else
            {
                controller.ChangeState(EnemyController.EnemyStateType.Alert);
            }
        }
    }
}