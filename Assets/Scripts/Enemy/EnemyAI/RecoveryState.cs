using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace PatrolEnemy
{
    public class RecoveryState : IEnemyState
    {
        private Vector3 coverPoint;
        private bool hasCoverPoint = false;
        private float lastHealthRecoveryTime = 0f;
        private float healthRecoveryInterval = 1f;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Recovery State");
            hasCoverPoint = false;
            lastHealthRecoveryTime = Time.time;
        }
        
        public void UpdateState(EnemyController controller)
        {
            // If health is recovered, return to appropriate state
            if (controller.CurrentHealth >= controller.MaxHealth)
            {
                DecideNextState(controller);
                return;
            }
            
            // If we don't have a cover point, find one
            if (!hasCoverPoint)
            {
                FindCoverPoint(controller);
            }
            
            // Check if we've reached the cover point
            if (hasCoverPoint && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                // Recover health over time
                if (Time.time >= lastHealthRecoveryTime + healthRecoveryInterval)
                {
                    float newHealth = Mathf.Min(controller.CurrentHealth + controller.RecoveryRate, controller.MaxHealth);
                    float recoveryAmount = newHealth - controller.CurrentHealth;
                    controller.CurrentHealth = newHealth;
                    lastHealthRecoveryTime = Time.time;
                    
                    Debug.Log($"Recovered {recoveryAmount} health. Current health: {controller.CurrentHealth}");
                }
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Recovery State");
        }
        
        private void FindCoverPoint(EnemyController controller)
        {
            // Find potential cover points around obstacles
            Collider[] obstacles = Physics.OverlapSphere(controller.transform.position, controller.DetectionRange);
            List<Vector3> potentialCoverPoints = new List<Vector3>();
            
            foreach (Collider obstacle in obstacles)
            {
                if (!obstacle.CompareTag("Player") && obstacle.gameObject != controller.gameObject)
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
            }
            
            // Find the closest valid cover point
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = controller.transform.position;
            
            foreach (Vector3 point in potentialCoverPoints)
            {
                float distance = Vector3.Distance(controller.transform.position, point);
                
                if (distance < closestDistance)
                {
                    // Check if this point is out of line of sight from player
                    Vector3 direction = (controller.CurrentTarget.position - point).normalized;
                    RaycastHit hit;
                    
                    if (Physics.Raycast(point, direction, out hit, controller.DetectionRange))
                    {
                        if (!hit.transform.CompareTag("Player"))
                        {
                            closestDistance = distance;
                            closestPoint = point;
                        }
                    }
                }
            }
            
            // If we found a valid cover point, head there
            if (closestDistance < float.MaxValue)
            {
                coverPoint = closestPoint;
                controller.Agent.SetDestination(coverPoint);
                hasCoverPoint = true;
                Debug.Log($"Moving to cover point: {coverPoint}");
            }
            else
            {
                // If no cover found, just move away from player
                Vector3 awayFromPlayer = controller.transform.position - controller.CurrentTarget.position;
                awayFromPlayer.y = 0;
                awayFromPlayer.Normalize();
                
                Vector3 retreatPoint = controller.transform.position + awayFromPlayer * 10f;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(retreatPoint, out hit, 10f, NavMesh.AllAreas))
                {
                    coverPoint = hit.position;
                    controller.Agent.SetDestination(coverPoint);
                    hasCoverPoint = true;
                    Debug.Log($"No cover found, retreating to: {coverPoint}");
                }
            }
        }
        
        private void DecideNextState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            if (distanceToPlayer <= controller.AttackRange)
            {
                if (controller.HasLineOfSight)
                {
                    controller.ChangeState(new ShootState());
                }
                else if (controller.CurrentGrenades > 0)
                {
                    controller.ChangeState(new GrenadeThrowState());
                }
                else
                {
                    controller.ChangeState(new FollowState());
                }
            }
            else if (distanceToPlayer <= controller.DetectionRange)
            {
                controller.ChangeState(new FollowState());
            }
            else
            {
                controller.ChangeState(new IdleState());
            }
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (hasCoverPoint)
            {
                // Draw path to cover point
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(coverPoint, 0.5f);
                
                if (controller.Agent.hasPath)
                {
                    Vector3[] corners = controller.Agent.path.corners;
                    for (int i = 0; i < corners.Length - 1; i++)
                    {
                        Gizmos.DrawLine(corners[i], corners[i + 1]);
                    }
                }
            }
            
            // Draw health recovery indicator
            Vector3 healthBarPos = controller.transform.position + Vector3.up * 2.5f;
            float healthPercentage = controller.CurrentHealth / controller.MaxHealth;
            Debug.DrawLine(healthBarPos, healthBarPos + Vector3.right * healthPercentage * 2f, Color.green);
        }
    }
}