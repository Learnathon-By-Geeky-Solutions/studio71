using UnityEngine;
using UnityEngine.AI;

namespace PatrolEnemy
{
    public class IdleState : IEnemyState
    {
        private Vector3 patrolPoint;
        private float patrolWaitTime = 2f;
        private float patrolTimer = 0f;
        private bool hasPatrolPoint = false;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Idle State");
            patrolTimer = 0f;
            hasPatrolPoint = false;
        }
        
        public void UpdateState(EnemyController controller)
        {
            // If player detected, switch to alert state
            if (controller.CurrentTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
                
                if (distanceToPlayer <= controller.DetectionRange)
                {
                    controller.ChangeState(new AlertState());
                    return;
                }
            }
            
            // Patrol behavior
            if (!hasPatrolPoint)
            {
                GetPatrolPoint(controller);
            }
            
            // Check if we've reached the patrol point
            if (hasPatrolPoint && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                // Wait for a bit at the patrol point
                patrolTimer += Time.deltaTime;
                
                if (patrolTimer >= patrolWaitTime)
                {
                    // Reset and get a new patrol point
                    hasPatrolPoint = false;
                    patrolTimer = 0f;
                }
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Idle State");
        }
        
        private void GetPatrolPoint(EnemyController controller)
        {
            // Generate a random point within the patrol range
            Vector2 randomCirclePoint = Random.insideUnitCircle * controller.PatrolRange;
            Vector3 randomPoint = controller.transform.position + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);
            
            // Try to find a valid NavMesh point
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, controller.PatrolRange, NavMesh.AllAreas))
            {
                patrolPoint = hit.position;
                controller.Agent.SetDestination(patrolPoint);
                hasPatrolPoint = true;
                Debug.Log($"Moving to patrol point: {patrolPoint}");
            }
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (hasPatrolPoint)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(patrolPoint, 0.5f);
                Gizmos.DrawLine(controller.transform.position, patrolPoint);
            }
        }
    }
}