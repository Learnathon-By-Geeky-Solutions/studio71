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
    private bool returningToZone = false;
        
        public void EnterState(EnemyController controller)
        {
           Debug.Log("Entered Idle State");
        patrolTimer = 0f;
        hasPatrolPoint = false;
        
        // Check if enemy is outside patrol zone
        float distanceFromStart = Vector3.Distance(controller.transform.position, controller.InitialPosition);
        if (distanceFromStart > controller.PatrolRange * 0.8f)
        {
            // Need to return to patrol zone first
            returningToZone = true;
            patrolPoint = controller.InitialPosition;
            controller.Agent.SetDestination(patrolPoint);
            hasPatrolPoint = true;
        }
        }
        
        public void UpdateState(EnemyController controller)
        {
            // If player detected, switch to alert state
        if (controller.CurrentTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            if (distanceToPlayer <= controller.DetectionRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Alert);
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
            if (returningToZone)
            {
                // We've returned to patrol zone, now patrol normally
                returningToZone = false;
                hasPatrolPoint = false;
            }
            else
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
        }

         private void GetPatrolPoint(EnemyController controller)
    {
        // Generate a random point within the patrol range from initial position
        Vector2 randomCirclePoint = Random.insideUnitCircle * controller.PatrolRange;
        Vector3 randomPoint = controller.InitialPosition + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);
        
        // Try to find a valid NavMesh point
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, controller.PatrolRange, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            controller.Agent.SetDestination(patrolPoint);
            hasPatrolPoint = true;
        }
    }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Idle State");
        }
        
    }
}