using UnityEngine;

namespace PatrolEnemy
{
    public class ShootState : IEnemyState
    {
        private float fireRate = 0.5f;
        private float nextFireTime = 0f;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Shoot State");
            nextFireTime = 0f;
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
            
            // If line of sight is lost, switch to grenade throw state if we have grenades
            if (!controller.HasLineOfSight && controller.CurrentGrenades > 0)
            {
                controller.ChangeState(new GrenadeThrowState());
                return;
            }
            
            // Look at player on Y axis only
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);
            
            // Shoot at player if we can
            if (Time.time >= nextFireTime && !controller.IsReloading)
            {
                controller.FireBullet();
                nextFireTime = Time.time + fireRate;
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Shoot State");
        }
        
        public void OnDrawGizmos(EnemyController controller)
        {
            if (controller.CurrentTarget != null)
            {
                // Draw attack line
                Gizmos.color = Color.red;
                Gizmos.DrawLine(controller.FirePoint.position, controller.CurrentTarget.position);
            }
        }
    }
}