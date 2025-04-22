using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using PatrolEnemy;

namespace PatrolEnemy
{
    public class GrenadeThrowState : IEnemyState
    {
        private CancellationTokenSource grenadeCTS;
        private bool hasThrown = false;
        
        public void EnterState(EnemyController controller)
        {
            Debug.Log("Entered Grenade Throw State");
            hasThrown = false;
            grenadeCTS = new CancellationTokenSource();
        }
        
        public void UpdateState(EnemyController controller)
        {
            if (controller.CurrentTarget == null)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Idle);
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
            
            // If player moved out of attack range, switch to follow state
            if (distanceToPlayer > controller.AttackRange)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
                return;
            }
            
            // If line of sight is gained, switch to shoot state
            if (controller.HasLineOfSight)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
                return;
            }
            
            // If we're out of grenades, switch to shoot state
            if (controller.CurrentGrenades <= 0)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Shoot);
                return;
            }
            
            // Look at player position
            Vector3 targetPosition = controller.CurrentTarget.position;
            targetPosition.y = controller.transform.position.y;
            controller.transform.LookAt(targetPosition);
            
            // Throw grenade if we haven't already
            if (!hasThrown && !controller.IsThrowingGrenade && controller.CurrentGrenades > 0)
            {
                ThrowGrenadeAsync(controller);
                hasThrown = true;
            }
            
            // Wait for cooldown then switch back to follow state
            if (hasThrown && !controller.IsThrowingGrenade)
            {
                controller.ChangeState(EnemyController.EnemyStateType.Follow);
            }
        }
        
        public void ExitState(EnemyController controller)
        {
            Debug.Log("Exited Grenade Throw State");
            grenadeCTS?.Cancel();
            grenadeCTS?.Dispose();
            grenadeCTS = null;
        }

        private async void ThrowGrenadeAsync(EnemyController controller)
        {
            if (controller.CurrentGrenades <= 0)
                return;
                
            controller.IsThrowingGrenade = true;
            
            // Try to find the ObjectPool instance in the scene
            ObjectPool objectPool = GameObject.FindObjectOfType<ObjectPool>();
            GameObject grenade;
            
            if (objectPool != null)
            {
                // Use object pooling
                grenade = objectPool.SpawnFromPool("Grenade", controller.GrenadePoint.position, controller.GrenadePoint.rotation);
                
                // Fallback if pool doesn't have "Grenade" tag
                if (grenade == null)
                {
                    grenade = GameObject.Instantiate(controller.GrenadePrefab, controller.GrenadePoint.position, controller.GrenadePoint.rotation);
                }
            }
            else
            {
                // Fallback to instantiation if no ObjectPool exists
                grenade = GameObject.Instantiate(controller.GrenadePrefab, controller.GrenadePoint.position, controller.GrenadePoint.rotation);
            }
            
            controller.CurrentGrenades--;
            Debug.Log($"Threw grenade. Grenades remaining: {controller.CurrentGrenades}");
            
            try
            {
                await Task.Delay((int)(controller.GrenadeThrowCooldown * 1000), grenadeCTS.Token);
                controller.IsThrowingGrenade = false;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Grenade cooldown interrupted");
                controller.IsThrowingGrenade = false;
            }
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