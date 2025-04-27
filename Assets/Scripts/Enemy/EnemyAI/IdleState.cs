using UnityEngine;
using UnityEngine.AI;

namespace PatrolEnemy
{
    public class IdleState : IEnemyState
    {
        private float patrolWaitTime = 5f;
        private float patrolTimer = 0f;
        private bool hasPatrolPoint;
        private bool returningToZone = false;

        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Idle State");
            patrolTimer = 0f;
            hasPatrolPoint = false;
            controller.IsIdleAlert = false;

            // Check if enemy needs to return to patrol zone
            float distanceFromStart = Vector3.Distance(controller.transform.position, controller.InitialPosition);
            if (distanceFromStart > controller.PatrolRange * 0.8f)
            {
                returningToZone = true;
                controller.Agent.SetDestination(controller.InitialPosition);
                hasPatrolPoint = true;
            }
        }

        public void UpdateState(EnemyController controller)
        {
            // Player detection check
            if (controller.CurrentTarget != null && 
                Vector3.Distance(controller.transform.position, controller.CurrentTarget.position) <= controller.DetectionRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Alert);
                return;
            }

            // Patrol behavior
            if (!hasPatrolPoint)
            {
                Vector3 patrolPoint = GetPatrolPoint(controller);
                controller.Agent.SetDestination(patrolPoint);
                hasPatrolPoint = true;
            }

            // Check if reached destination
            if (hasPatrolPoint && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                if (returningToZone)
                {
                    returningToZone = false;
                    hasPatrolPoint = false;
                }
                else
                {
                    if (!controller.IsIdleAlert)
                    {
                        // Immediately trigger alert animation after reaching
                        controller.IsIdleAlert = true;
                        patrolTimer = 0f;
                    }

                    patrolTimer += Time.deltaTime;
                    if (patrolTimer >= patrolWaitTime)
                    {
                        // After pause, back to normal patrol
                        controller.IsIdleAlert = false;
                        hasPatrolPoint = false;
                        patrolTimer = 0f;
                    }
                }
            }
        }

        private Vector3 GetPatrolPoint(EnemyController controller)
        {
            // Generate random point within patrol range
            Vector2 randomCirclePoint = Random.insideUnitCircle * controller.PatrolRange;
            Vector3 randomPoint = controller.InitialPosition + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);

            // Validate NavMesh position
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, controller.PatrolRange, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return controller.InitialPosition; // Fallback
        }

        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Idle State");
        }
    }
}
