using UnityEngine;

namespace PatrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private bool hasThrown = false;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Grenade Throw State");
            hasThrown = false;
        }
        
        public void UpdateState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(new IdleState());
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            // If player moved out of attack range, switch to follow state
            if (distanceToPlayer > controller.AttackRange)
            {
                controller.ChangeState(new FollowState());
                return;
            }
            
            // If line of sight is gained, switch to shoot state
            if (controller.HasLineOfSight)
            {
                controller.ChangeState(new ShootState());
                return;
            }
            
            // If we're out of grenades, switch to shoot state
            if (controller.CurrentGrenades <= 0)
            {
                controller.ChangeState(new ShootState());
                return;
            }
            
            // Look at player position
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);
            
            // Throw grenade if we haven't already
            if (!hasThrown && !controller.IsThrowingGrenade)
            {
                controller.ThrowGrenade();
                hasThrown = true;
            }
            
            // Wait for cooldown then switch back to follow state
            if (hasThrown && !controller.IsThrowingGrenade)
            {
                controller.ChangeState(new FollowState());
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Grenade Throw State");
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (controller.CurrentTarget != null)
            {
                // Draw grenade trajectory
                Gizmos.color = Color.yellow;
                Vector3 grenadeStart = controller.GrenadePoint.position;
                Vector3 grenadeEnd = controller.CurrentTarget.position;
                
                // Simple arc
                Vector3 mid = (grenadeStart + grenadeEnd) / 2f + Vector3.up * 2f;
                
                const int segments = 20;
                Vector3 prev = grenadeStart;
                for (int i = 1; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = Vector3.Lerp(Vector3.Lerp(grenadeStart, mid, t), 
                                               Vector3.Lerp(mid, grenadeEnd, t), t);
                    Gizmos.DrawLine(prev, point);
                    prev = point;
                }
            }
        }
    }
}