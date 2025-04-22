using UnityEngine;

namespace PatrolEnemy
{
    public class FollowState : IEnemyState
    {
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Follow State");
        }
        
        public void UpdateState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            // If player moved out of detection range, return to idle
            if (distanceToPlayer > controller.DetectionRange)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            // If player is within attack range, switch to appropriate attack state
            if (distanceToPlayer <= controller.AttackRange)
            {
                if (controller.HasLineOfSight)
                {
                    controller.ChangeState(new ShootState());
                }
                else
                {
                    // Only switch to grenade throw if we have grenades
                    if (controller.CurrentGrenades > 0)
                    {
                        controller.ChangeState(new GrenadeThrowState());
                    }
                    else
                    {
                        // Try to move to get line of sight
                        controller.Agent.SetDestination(controller.CurrentTarget.position);
                    }
                }
                return;
            }
            
            // Follow the player
            controller.Agent.SetDestination(controller.CurrentTarget.position);
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Follow State");
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (controller.CurrentTarget != null)
            {
                // Draw path to player
                Gizmos.color = Color.blue;
                if (controller.Agent.hasPath)
                {
                    Vector3[] corners = controller.Agent.path.corners;
                    for (int i = 0; i < corners.Length - 1; i++)
                    {
                        Gizmos.DrawLine(corners[i], corners[i + 1]);
                    }
                }
            }
        }
    }
}